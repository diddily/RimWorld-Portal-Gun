using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Buildings;


namespace Portal_Gun.Harmony
{

    [HarmonyPatch(typeof(Verse.RegionMaker), "TryGenerateRegionFrom")]
    public class RegionMaker_TryGenerateRegionFrom
    {
        static void Postfix(Region __result, IntVec3 root, Map ___map)
        {
            Map map = ___map;
            if (root.InBounds(map))
            {
                Region rootRegion = map.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(root);
                if (rootRegion != null)
                {
                    foreach (Building_PortalGunPortalEntry pe in rootRegion.Cells.SelectMany(c => c.GetThingList(map).OfType<Building_PortalGunPortalEntry>().Where(p => p.linkedPortal != null && p.linkedPortal.LinkedPortal != null)))
                    {
                        Building_PortalGunPortalEntry primaryPortalEntry = pe;
                        if (!pe.linkedPortal.OwnsRegion)
                        {
                            primaryPortalEntry = pe.linkedPortal.LinkedPortal.linkedEntry;
                        }

                        IntVec3 primaryPosition = primaryPortalEntry.Position;
                        IntVec3 secondaryPosition = primaryPortalEntry.linkedPortal.LinkedPortal.linkedEntry.Position;

                        Log.Message("Link attempt for " + primaryPosition + " to " + secondaryPosition);
                        if (primaryPosition.InBounds(map) && secondaryPosition.InBounds(map))
                        {
                            Region primaryRegion = map.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(primaryPosition);
                            Region secondaryRegion = map.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(secondaryPosition);
                            Log.Message("primaryRegion " + primaryRegion + " secondaryRegion " + secondaryRegion);
                            if (primaryRegion != null && secondaryRegion != null && primaryRegion != secondaryRegion)
                            {
                                EdgeSpan span = new EdgeSpan(primaryPosition, SpanDirection.East, 0);
                                RegionLink regionLink = map.regionLinkDatabase.LinkFrom(span);
                                Log.Message("regionLink.RegionA " + regionLink.RegionA + " regionLink.RegionB " + regionLink.RegionB);
                                if (regionLink.RegionA == null && regionLink.RegionB == null)
                                {
                                    regionLink.Register(primaryRegion);
                                    regionLink.Register(secondaryRegion);
                                    primaryRegion.links.Add(regionLink);
                                    secondaryRegion.links.Add(regionLink);
                                }
                                else if (regionLink.RegionA != null && regionLink.RegionA.valid && (regionLink.RegionB == null || !regionLink.RegionB.valid))
                                {
                                    if (primaryRegion == regionLink.RegionA)
                                    {
                                        regionLink.RegionB = secondaryRegion;
                                        secondaryRegion.links.Add(regionLink);
                                    }
                                    else
                                    {
                                        regionLink.RegionB = primaryRegion;
                                        primaryRegion.links.Add(regionLink);
                                    }
                                    Log.Message("regionLink.RegionB assigned to " + regionLink.RegionB);
                                }
                                else if (regionLink.RegionB != null && regionLink.RegionB.valid && (regionLink.RegionA == null || !regionLink.RegionA.valid))
                                {
                                    if (primaryRegion == regionLink.RegionB)
                                    {
                                        regionLink.RegionA = secondaryRegion;
                                        secondaryRegion.links.Add(regionLink);
                                    }
                                    else
                                    {
                                        regionLink.RegionA = primaryRegion;
                                        primaryRegion.links.Add(regionLink);
                                    }
                                    Log.Message("regionLink.RegionA assigned to " + regionLink.RegionB);
                                }
                                else
                                {
                                    Log.Message("Did fuck all");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
