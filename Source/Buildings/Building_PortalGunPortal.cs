﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Portal_Gun.Items;
using UnityEngine;

namespace Portal_Gun.Buildings
{
	public class Building_PortalGunPortal : Building
	{
		static public ColorInt Blue = new ColorInt(39, 167, 216, 255);
		static public ColorInt Orange = new ColorInt(255, 154, 0, 255);

		public Color PortalColor;
		private bool __ownsRegion;
		public bool OwnsRegion
		{
			get
			{
				return __ownsRegion;
			}
			set
			{
				__ownsRegion = value;
				if (LinkedPortal != null)
				{
					LinkedPortal.__ownsRegion = !value;
				}
			}
		}
		private Building_PortalGunPortal __linkedPortal;
		public Building_PortalGunPortal LinkedPortal
		{
			get
			{
				return __linkedPortal;
			}
			set
			{
				if (__linkedPortal != value)
				{
					if (linkedEntry != null)
					{
						linkedEntry.DeSpawn(0);
						linkedEntry = null;
					}

					__linkedPortal = value;

					if (__linkedPortal != null)
					{
						linkedEntry = (Building_PortalGunPortalEntry)ThingMaker.MakeThing(PG_DefOf.PG_PortalEntry);
						GenSpawn.Spawn(linkedEntry, IsWall ? Position + Rotation.FacingCell : Position, Map, 0);
						linkedEntry.linkedPortal = this;
						UpdateGlowerProps();
						
						__linkedPortal.LinkedPortal = this;
						if (__ownsRegion == __linkedPortal.__ownsRegion)
						{
							OwnsRegion = __ownsRegion;
						}
					}
				}
			}
		}

		private void UpdateGlowerProps()
		{
			CompProperties_Glower newGlowProps = new CompProperties_Glower();
			CompProperties_Glower oldGlowProps = linkedEntry.GetComp<CompGlower>().props as CompProperties_Glower;
			newGlowProps.compClass = oldGlowProps.compClass;
			newGlowProps.glowColor = new ColorInt(PortalColor);
			newGlowProps.glowRadius = oldGlowProps.glowRadius;
			newGlowProps.overlightRadius = oldGlowProps.overlightRadius;
			linkedEntry.GetComp<CompGlower>().props = newGlowProps;
		}

		public override void Tick()
		{
			base.Tick();

			if (!Retest())
			{
				Destroy();
				Messages.Message(string.Format("PG_PortalCreationFailed".Translate(), def.LabelCap, "PG_SurfaceIncompatible_Changed".Translate()), this, MessageTypeDefOf.RejectInput, false);
			}
		}

		public bool Retest()
		{
			List<Thing> things = Position.GetThingList(Map);
			bool isWall = things.Count(t => t.def.coversFloor) > 0;
			bool wasWall = IsWall;

			if (isWall != wasWall)
			{
				Portal_Gun.Message("isWall != wasWall");
				return false;
			}

			if (GenGrid.Impassable(IsWall ? Position + Rotation.FacingCell : Position, Map))
			{
				Portal_Gun.Message("GenGrid.Impassable(linkedEntry.Position, Map)");
				return false;
			}
			if (isWall)
			{
				// TODO: Detect smoothing or other changes to a wall.
				bool notSolid = things.Count(t => t.def.fillPercent > 0 && t.def.fillPercent < 1) > 0;
				if (notSolid)
				{
					Portal_Gun.Message("notSolid");
					return false;
				}
			}
			else
			{
				TerrainDef terrain = Position.GetTerrain(Map);
				if (terrain != Terrain)
				{
					Portal_Gun.Message("terrain != Terrain");
					return false;
				}
			}

			return true;
		}

		public bool IsLocalPortal
		{
			get
			{
				return LinkedPortal != null && LinkedPortal.worldTile == worldTile;
			}
		}

		private TerrainDef __terrain;
		public TerrainDef Terrain { get { return __terrain; } set { __terrain = value; } }
		public Item_PortalGun linkedGun;
		public Building_PortalGunPortalEntry linkedEntry;
		public int worldTile = -1;
		public float powerDrawScale = 1.0f;
		private bool cleaningUp;
		private bool __isWall;
		public bool IsWall { get { return __isWall; } set { __isWall = value; } }
		private float __floorRotation;
		public float FloorRotation { get { return __floorRotation; } set { __floorRotation = value; } }
		public bool IsConnected
		{
			get
			{
				return LinkedPortal != null;
			}
		}
		public override Color DrawColor { get => PortalColor; set => PortalColor = value; }

		private void Cleanup()
		{
			if (cleaningUp)
			{
				return;
			}
			cleaningUp = true;
			if (LinkedPortal != null)
			{
				LinkedPortal.LinkedPortal = null;
				LinkedPortal = null;
			}

			if (linkedGun != null)
			{
				linkedGun.Notify_PortalRemoved(this);
				linkedGun = null;
			}

			if (linkedEntry != null)
			{
				linkedEntry.DeSpawn(DestroyMode.Vanish);
				linkedEntry = null;
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Cleanup();

			base.DeSpawn(mode);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			worldTile = map.Tile;
			base.SpawnSetup(map, respawningAfterLoad);
			if (Terrain == null)
			{
				Terrain = Position.GetTerrain(map);
			}
			if (linkedEntry != null)
			{
				UpdateGlowerProps();
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (Prefs.DevMode)
			{

				yield return new Command_Target
				{
					defaultLabel = "Select Linked Portal",
					defaultDesc = "Select Linked Portal",
					targetingParams = new TargetingParameters
					{
						canTargetPawns = false,
						canTargetBuildings = true,
						mapObjectTargetsMustBeAutoAttackable = false,
						validator = ((TargetInfo x) => x.Thing is Building_PortalGunPortal && x.Thing != this)
					},
					action = delegate (LocalTargetInfo targetInfo)
					{
						Building_PortalGunPortal otherPortal = targetInfo.Thing as Building_PortalGunPortal;
						otherPortal.LinkedPortal = this;
						this.LinkedPortal = otherPortal;
					}
				};
			}
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (LinkedPortal != null)
			{
				GenDraw.DrawLineBetween(LinkedPortal.TrueCenter(), this.TrueCenter());
			}

			if (this.linkedGun != null)
			{
				GenDraw.DrawLineBetween(linkedGun.SpawnedParentOrMe.TrueCenter(), this.TrueCenter());
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref PortalColor, "PortalColor");
			Scribe_Values.Look(ref __isWall, "__isWall");
			Scribe_Defs.Look(ref __terrain, "__terrain");
			Scribe_Values.Look(ref __floorRotation, "__floorRotation");
			Scribe_Values.Look(ref __ownsRegion, "__ownsRegion");
			Scribe_Values.Look(ref worldTile, "worldTile", -1);
			Scribe_Values.Look(ref powerDrawScale, "powerDrawScale", 1);
			Scribe_References.Look(ref __linkedPortal, "__linkedPortal");
			Scribe_References.Look(ref linkedGun, "linkedGun");
			Scribe_References.Look(ref linkedEntry, "linkedEntry");
		}
	}
}