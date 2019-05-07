using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using Portal_Gun.Buildings;
using Portal_Gun.Comps;
using Portal_Gun.Projectiles;

namespace Portal_Gun.Items
{
    public class Item_PortalGun : ThingWithComps, IThingHolder
    {
        public bool isSecondary;

        private ThingOwner innerContainer;
        public Building_PortalGunPortal primaryPortal;
        public Building_PortalGunPortal secondaryPortal;
        
        private List<Verb> verbs;
        private VerbTracker fakeVerbTracker;

        private Comp_PortalGun compPortalGun => GetComp<Comp_PortalGun>();
        private CompEquippable compEquippable => GetComp<CompEquippable>();
        private CompFacility compFacility => GetComp<CompFacility>();
        private CompRefuelable compRefuelable => GetComp<CompRefuelable>();

        private CompRefuelable fakeRefuelable;

        // Updated by tick
        private float __powerRequired;
        private float __powerInternal;
        private float __powerExternal;
        private float __powerCharging;
        private float __powerCapacity = 1.0f;
        private float __powerMultiply = 1.0f;
        private bool __hasExternalPower;

        private static FieldInfo linkedBuildingsField = AccessTools.Field(typeof(CompFacility), "linkedBuildings");

        public Item_PortalGun()
        {
            innerContainer = new ThingOwner<Thing>(this);
            fakeRefuelable = new CompRefuelable();
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public float PowerCharging
        {
            get { return __powerCharging; }
        }

        public float PowerPercent
        {
            get { return compRefuelable.FuelPercentOfMax; }
        }

        public bool HasPower
        {
            get { return compRefuelable.HasFuel; }
        }

        public float PowerExternal
        {
            get
            {
                return __powerExternal;
            }
        }

        public float PowerRequired
        {
            get
            {
                return __powerRequired;
            }
        }

        public bool HasAdventureCoreModule
        {
            get
            {
                return innerContainer.Any(t => t.def == PG_DefOf.PG_AdventureCoreModule);
            }
        }
        public bool HasViolenceCoreModule
        {
            get
            {
                return innerContainer.Any(t => t.def == PG_DefOf.PG_ViolenceCoreModule);
            }
        }

        public bool HasGLaDOSModule
        {
            get
            {
                return innerContainer.Any(t => t.def == PG_DefOf.PG_GLaDOSModule);
            }
        }

        public int SurfaceLevel
        {
            get
            {
                return innerContainer.Count > 0 ? innerContainer.Select(t => t.TryGetComp<Comp_PortalGunModule>()).Max(pgm => pgm == null ? 0 : pgm.Props.surfaceLevel) : 0;
            }
        }


        public List<Verb> Verbs
        {
            get
            {
                if (verbs == null)
                {
                    InitVerbsFromZero();
                }
                return verbs;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (compRefuelable.HasFuel)
                {
                    if (isSecondary)
                    {
                        if (HasGLaDOSModule)
                        {
                            return compPortalGun.PG_Props.secondaryGLaDOSGraphic.Graphic;
                        }
                        else
                        {
                            return compPortalGun.PG_Props.secondaryGraphic.Graphic;
                        }
                    }
                    else
                    {
                        if (HasGLaDOSModule)
                        {
                            return compPortalGun.PG_Props.primaryGLaDOSGraphic.Graphic;
                        }
                        else
                        {
                            return compPortalGun.PG_Props.primaryGraphic.Graphic;
                        }
                    }
                }
                else
                {
                    if (HasGLaDOSModule)
                    {
                        return compPortalGun.PG_Props.offGLaDOSGraphic.Graphic;
                    }
                    else
                    {
                        return compPortalGun.PG_Props.offGraphic.Graphic;
                    }
                }
            }
        }

        public void ClearPortals()
        {
            if (primaryPortal != null)
            {
                primaryPortal.Destroy(DestroyMode.Vanish);
            }

            if (secondaryPortal != null)
            {
                secondaryPortal.Destroy(DestroyMode.Vanish);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            ClearPortals();

            base.Destroy(mode);
        }

        public bool CreatePortal(Projectile_PortalGun projectile, Vector3 offset, IntVec3 targetPos, Map map)
        {
            if (projectile.def == PG_DefOf.PG_PortalGun_BulletSecondary)
            {
                return CreatePortal(ref secondaryPortal, targetPos, map, offset, ref primaryPortal, true);
            }
            else
            {
                return CreatePortal(ref primaryPortal, targetPos, map, offset, ref secondaryPortal, false);
            }
        }

        private bool CreatePortal(ref Building_PortalGunPortal portal, IntVec3 location, Map map, Vector3 offset, ref Building_PortalGunPortal otherPortal, bool secondaryVal)
        {
            List<Thing> things = location.GetThingList(map);
            bool isWall = things.Count(t => t.def.coversFloor) > 0;
            bool obstructed = things.Count(t => !(t.def.coversFloor || (t.def.passability == Traversability.Standable && t.def.pathCost == 0 && t.def.fillPercent == 0))) > 0;
            ThingDef portalDef = isWall ? PG_DefOf.PG_WallPortal : PG_DefOf.PG_FloorPortal;
            bool success = true;
            float angleFlat = offset.AngleFlat();
            Rot4 rotation = Rot4.FromAngleFlat(offset.AngleFlat());
            IntVec3 facing = rotation.FacingCell;
            IntVec3 entryPos = location;
            int requiredSurfaceLevel = 0;
            string messageText = null;

            if (obstructed)
            {
                messageText = "PG_SurfaceIncompatible_AlreadyOccupied";
                success = false; 
            }

            if (isWall)
            {
                entryPos += facing;

                bool tooRough = things.Count(t => (t.def.building != null && t.def.building.isNaturalRock == true) || t.def == ThingDefOf.CollapsedRocks) > 0;
                bool notSolid = things.Count(t => t.def.fillPercent > 0 && t.def.fillPercent < 1) > 0;
                if (success && notSolid)
                {
                    messageText = "PG_SurfaceIncompatible_NotSolid";
                    success = false;
                }
                if (success && tooRough)
                {
                    messageText = "PG_SurfaceIncompatible_TooRough";
                    requiredSurfaceLevel = 1;
                }
            }
            else
            {
                TerrainDef terrain = entryPos.GetTerrain(map);
                if (terrain.BuildableByPlayer)
                {
                    if (terrain.IsCarpet)
                    {
                        messageText = "PG_SurfaceIncompatible_TooSoft";
                        requiredSurfaceLevel = 2;
                    }
                }
                else
                {
                    // Water never supports portals, anything else "wet" enough to be bridgeable can support portals at level 2
                    if (terrain.IsWater)
                    {
                        messageText = "PG_SurfaceIncompatible_TooWet";
                        success = false;
                    }
                    else if (terrain.affordances.Contains(DefDatabase<TerrainAffordanceDef>.GetNamed("Bridgeable", true)))
                    {
                        messageText = "PG_SurfaceIncompatible_TooWet";
                        requiredSurfaceLevel = 2;
                    }
                    else if (terrain.smoothedTerrain != null || terrain.HasTag("Road"))
                    {
                        messageText = "PG_SurfaceIncompatible_TooRough";
                        requiredSurfaceLevel = 1;
                    }
                    else if (terrain.affordances.Contains(TerrainAffordanceDefOf.Diggable))
                    {
                        messageText = "PG_SurfaceIncompatible_TooLoose";
                        requiredSurfaceLevel = 2;
                    }
                }
                rotation = Rot4.North;
            }
            //map.debugDrawer.FlashCell(entryPos, 0.55f, "firstTest", 100);

            if (GenGrid.Impassable(entryPos, map))
            {
                //Log.Message("Facing = " + facing + " Offset = " + offset);
                if (!isWall)
                {
                    messageText = "PG_SurfaceIncompatible_LocationBlocked";
                    success = false;
                }
                else if (facing.z == 0)
                {
                    if (offset.z >= 1)
                    {
                        facing = IntVec3.North;
                    }
                    else if (offset.z <= -1)
                    {
                        facing = IntVec3.South;
                    }
                    else
                    {
                        success = false;
                    }
                }
                else
                {
                    if (offset.x >= 1)
                    {
                        facing = IntVec3.East;
                    }
                    else if (offset.x <= -1)
                    {
                        facing = IntVec3.West;
                    }
                    else
                    {
                        messageText = "PG_SurfaceIncompatible_LocationBlocked";
                        success = false;
                    }
                }
                if (success)
                {
                    rotation = Rot4.FromIntVec3(facing);
                    entryPos = location + facing;
                    //map.debugDrawer.FlashCell(entryPos, 0.22f, "secondTest", 100);
                    
                    if (!GenGrid.Impassable(entryPos, map))
                    {
                        messageText = "PG_SurfaceIncompatible_LocationBlocked";
                        success = false;
                    }
                }
            }

            if (success && SurfaceLevel < requiredSurfaceLevel)
            {
                success = false;
            }

            if (success && (otherPortal != null && otherPortal.worldTile != map.Tile && !HasAdventureCoreModule))
            {
                messageText = "PG_TooFar";
                success = false;
            }

            if (success && things.Any(t => t.def == portalDef && t.Rotation == rotation))
            {
                messageText = "PG_SurfaceIncompatible_LocationBlocked";
                success = false;
            }

            if (!success && messageText != null)
            {
                Messages.Message(string.Format("PG_PortalCreationFailed".Translate(), portalDef.LabelCap, messageText.Translate()), this, MessageTypeDefOf.RejectInput, false);
            }

            if (success)
            {
                if (portal != null && !portal.Destroyed)
                {
                    portal.Destroy(DestroyMode.Vanish);
                    // portal should be null now.
                }

                portal = (Building_PortalGunPortal)GenSpawn.Spawn(ThingMaker.MakeThing(portalDef), location, map, rotation);
                portal.SetFaction(SpawnedParentOrMe.Faction);
                portal.IsWall = isWall;
                portal.FloorRotation = angleFlat;
                if (requiredSurfaceLevel == 2)
                {
                    portal.powerDrawScale = 1.25f;
                }
                else if (requiredSurfaceLevel == 1)
                {
                    portal.powerDrawScale = 1.125f;
                }
                if (secondaryVal)
                {
                    portal.DrawColor = Building_PortalGunPortal.Orange.ToColor;
                }
                else
                {
                    portal.DrawColor = Building_PortalGunPortal.Blue.ToColor;
                }

                portal.LinkedPortal = otherPortal;
                portal.IsWall = portalDef == PG_DefOf.PG_WallPortal;
                if (otherPortal != null)
                {
                    otherPortal.LinkedPortal = portal;
                }
                portal.linkedGun = this;
                portal.OwnsRegion = true;
            }

            return success;
        }

        public void Notify_PortalRemoved(Building_PortalGunPortal portal)
        {
            if (portal == primaryPortal)
            {
                primaryPortal = null;
            }

            if (portal == secondaryPortal)
            {
                secondaryPortal = null;
            }
        }

        private void UpdatePower()
        {
            Building charger = (linkedBuildingsField.GetValue(compFacility) as List<Thing>).OfType<Building>().FirstOrDefault();
            if (charger != null)
            {
                __hasExternalPower = charger.GetComp<CompPowerPlant_PortalGunCharger>().PowerOutput != 0;
            }
            else
            {
                __hasExternalPower = false;
            }

            __powerRequired = compPortalGun.PG_Props.basePowerDraw;
            if (primaryPortal != null)
            {
                __powerRequired += compPortalGun.PG_Props.portalPowerDraw * primaryPortal.powerDrawScale;
            }
            if (secondaryPortal != null)
            {
                __powerRequired += compPortalGun.PG_Props.portalPowerDraw * secondaryPortal.powerDrawScale;
            }

            __powerCharging = 0;
            __powerExternal = 0;
            float powerDeficit = __powerInternal - __powerRequired;
            if (powerDeficit > 0 || __hasExternalPower)
            {
                float maxChargeDraw = 0;
                if (!compRefuelable.IsFull)
                {
                    maxChargeDraw = compPortalGun.PG_Props.rechargePowerDraw;
                }

                __powerCharging = Math.Min(maxChargeDraw, powerDeficit);

                if (__hasExternalPower)
                {
                    __powerExternal = maxChargeDraw - __powerCharging;
                    __powerCharging = maxChargeDraw;
                }
            }
            else if (compRefuelable.HasFuel)
            {
                __powerCharging = powerDeficit;
            }

            if (__powerCharging > 0)
            {
                compRefuelable.Refuel(__powerCharging * CompPower.WattsToWattDaysPerTick);
            }
            else
            {
                compRefuelable.ConsumeFuel(-__powerCharging * CompPower.WattsToWattDaysPerTick);
            }
            //__storedPower = Math.Max(0, Math.Min(compPortalGun.PG_Props.maxStoredPower, __storedPower + __powerCharging * CompPower.WattsToWattDaysPerTick));
            
        }

        public void UpdateModules(Comp_PortalGunModule removedModule)
        {
           
            float totalEnergy = compRefuelable.Fuel;
            float totalStorage = compRefuelable.Props.fuelCapacity;
            __powerInternal = 0.0f;
            __powerMultiply = 1.0f;

            if (__powerCapacity > 0.0f)
            {
                totalEnergy /= __powerCapacity;
            }

            foreach (Comp_PortalGunModule pgm in innerContainer.Select(t => t.TryGetComp<Comp_PortalGunModule>()))
            {
                totalEnergy += pgm.storedEnergy;
                pgm.storedEnergy = 0.0f;
                totalStorage += pgm.Props.powerStorage;
                __powerInternal += pgm.Props.powerOutput;
                __powerMultiply -= pgm.Props.powerEfficiency;
            }

            if (totalEnergy > totalStorage && removedModule != null)
            {
                removedModule.storedEnergy = Math.Max(totalEnergy - totalStorage, removedModule.Props.powerStorage);
                totalEnergy -= removedModule.storedEnergy;
            }

            __powerCapacity = compRefuelable.Props.fuelCapacity / totalStorage;
            float scaledEnergy = totalEnergy * __powerCapacity;
            float deltaFuel = scaledEnergy - compRefuelable.Fuel;

            if (deltaFuel > 0)
            {
                compRefuelable.Refuel(deltaFuel);
            }
            else if (deltaFuel < 0)
            {
                compRefuelable.ConsumeFuel(-deltaFuel);
            }
        }

        public void AddModule(Thing thing)
        {
            innerContainer.TryAddOrTransfer(thing);

            UpdateModules(null);
        }

        public void RemoveModule(Thing thing, Pawn remover)
        {
            if (innerContainer.TryTransferToContainer(thing, remover.carryTracker.GetDirectlyHeldThings()))
            {
                UpdateModules(thing.TryGetComp<Comp_PortalGunModule>());
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            if (innerContainer == null)
            {
                innerContainer = new ThingOwner<Thing>(this);
            }

            innerContainer.ThingOwnerTick(true);

            UpdatePower();

        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RanOutOfFuel")
            {
                PG_DefOf.PG_PortalGunOutOfPower.PlayOneShot(new TargetInfo(this));
                ClearPortals();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Find.Selector.SingleSelectedThing == SpawnedParentOrMe)
            {
                yield return new Gizmo_RefuelableFuelStatus
                {
                    refuelable = compRefuelable
                };
            }
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (this.verbs == null)
            {
                InitVerbsFromZero();
            }

            for (int i = 0; i < verbs.Count; i++)
            {
                Verb verb = verbs[i];
                if (verb.verbProps.hasStandardCommand)
                {
                    Command_VerbTarget command_VerbTarget = new Command_VerbTarget();

                    command_VerbTarget.defaultLabel = verb.verbProps.defaultProjectile.description;
                    command_VerbTarget.defaultDesc = verb.verbProps.defaultProjectile.description;
                    command_VerbTarget.icon = verb.verbProps.defaultProjectile.uiIcon;
                    command_VerbTarget.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
                    command_VerbTarget.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
                    command_VerbTarget.hotKey = null;
                    command_VerbTarget.verb = verb;
                    if (verb.caster.Faction != Faction.OfPlayer)
                    {
                        command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
                    }
                    yield return command_VerbTarget;
                }
            }
            if (primaryPortal != null || secondaryPortal != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Clear Portals",
                    defaultDesc = "Destroys any portals linked to this gun in the world",
                    action = delegate ()
                    {
                        ClearPortals();
                    }
                };
            }
            /*
            yield return new Command_Target
            {
                defaultLabel = "Create Secondary Portal",
                defaultDesc = "Shoot the gun in order to create a new portal",
                targetingParams = new TargetingParameters
                {
                    canTargetPawns = false,
                    canTargetBuildings = true,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    neverTargetDoors = true,
                    validator = ((TargetInfo x) => x.Thing != null && (x.Thing.def == ThingDefOf.Wall || x.Thing.def.IsSmoothed))
                },
                action = delegate (Thing target)
                {
                    CreatePortal(ref secondaryPortal, PG_DefOf.PG_WallPortal, target, ref primaryPortal, true);
                }
            };*/
        }

        private void InitVerbsFromZero()
        {
            verbs = new List<Verb>();
            fakeVerbTracker = new VerbTracker(compEquippable);
            InitVerbs(delegate (Type type, string id)
            {
                Verb verb = (Verb)Activator.CreateInstance(type);
                verbs.Add(verb);
                return verb;
            });

            AccessTools.Field(typeof(VerbTracker), "verbs").SetValue(fakeVerbTracker, verbs);
        }

        public FloatMenuOption GetFloatOptionForModule(Pawn selPawn, Thing module)
        {
            FloatMenuOption useopt = new FloatMenuOption("PG_RemoveModule".Translate() + ": " + module.Label, delegate
            {
                if (selPawn.CanReserveAndReach(this, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                {
                    Job job = new Job(PG_DefOf.PG_JobRemoveModule, module, this);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }, MenuOptionPriority.Default, null, null, 0f, null, null);

            return useopt;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(selPawn))
            {
                yield return o;
            }
            if (!selPawn.CanReach(this, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption("PG_RemoveModule".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!selPawn.CanReserve(this, 1, -1, null, false))
            {
                yield return new FloatMenuOption("PG_RemoveModule".Translate() + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                yield return new FloatMenuOption("PG_RemoveModule".Translate() + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                foreach (Thing module in GetDirectlyHeldThings())
                {
                    yield return GetFloatOptionForModule(selPawn, module);
                }
            }

        }

        private void InitVerbs(Func<Type, string, Verb> creator)
        {
            List<VerbProperties> verbProperties = compPortalGun.PG_Props.verbs;
            if (verbProperties != null)
            {
                for (int i = 0; i < verbProperties.Count; i++)
                {
                    try
                    {
                        VerbProperties verbProperties2 = verbProperties[i];
                        string text = Verb.CalculateUniqueLoadID(GetComp<CompEquippable>(), i);
                        this.InitVerb(creator(verbProperties2.verbClass, text), verbProperties2, null, null, text);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(string.Concat(new object[]
                        {
                            "Could not instantiate Verb (fakeOwner=",
                            compEquippable.ToStringSafe<IVerbOwner>(),
                            "): ",
                            ex
                        }), false);
                    }
                }
            }
        }

        private void InitVerb(Verb verb, VerbProperties properties, Tool tool, ManeuverDef maneuver, string id)
        {
            verb.loadID = id;
            verb.verbProps = properties;
            verb.verbTracker = fakeVerbTracker;
            verb.tool = tool;
            verb.maneuver = maneuver;
            verb.caster = SpawnedParentOrMe;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isSecondary, "isSecondary");
            Scribe_Values.Look(ref __powerRequired, "__powerRequired");
            Scribe_Values.Look(ref __powerInternal, "__powerInternal");
            Scribe_Values.Look(ref __powerExternal, "__powerExternal");
            Scribe_Values.Look(ref __powerCharging, "__powerCharging");
            Scribe_Values.Look(ref __powerMultiply, "__powerMultiply", 1.0f);
            Scribe_Values.Look(ref __powerCapacity, "__powerCapacity", 1.0f);
            //Scribe_Values.Look(ref __storedPower, "__storedPower");
            Scribe_Values.Look(ref __hasExternalPower, "__hasExternalPower");
            Scribe_References.Look(ref secondaryPortal, "secondaryPortal");
            Scribe_References.Look(ref primaryPortal, "primaryPortal");
            Scribe_Deep.Look<ThingOwner>(ref innerContainer, "innerContainer", new object[]
            {
                this
            });
        }
    }


}
