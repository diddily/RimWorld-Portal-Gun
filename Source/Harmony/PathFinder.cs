using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Buildings;
using Portal_Gun.Jobs;

namespace Portal_Gun.Harmony
{
    [HarmonyPatch(typeof(PathFinder), "FindPath")]
    [HarmonyPatch(new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode) })]
    public class PathFinder_FindPath
    {
        private static FieldInfo mapField = AccessTools.Field(typeof(PathFinder), "map");
        private static FieldInfo pawnField = AccessTools.Field(typeof(Pawn), "pawn");

        private static FieldInfo intVec3XField = AccessTools.Field(typeof(IntVec3), "x");
        private static FieldInfo intVec3ZField = AccessTools.Field(typeof(IntVec3), "z");
        private static FieldInfo statusClosedValueField = AccessTools.Field(typeof(PathFinder), "statusClosedValue");
        private static FieldInfo statusOpenValueField = AccessTools.Field(typeof(PathFinder), "statusOpenValue");

        public static List<Building_PortalGunPortalEntry> GetPortals(Map map, ref IntVec3 cell)
        {
            return cell.GetThingList(map).OfType<Building_PortalGunPortalEntry>().Where(pe => pe.IsConnected).ToList();
        }

        public static int GetPortalsCount(List<Building_PortalGunPortalEntry> p)
        {
            return p.Count;
        }
        public static void GetExit(List<Building_PortalGunPortalEntry> p, int i, ref int x, ref int z)
        {
            Building_PortalGunPortalEntry exit = p[p.Count + i].Exit;
            x = exit.Position.x;
            z = exit.Position.z;
        }
        public static int GetExitX(List<Building_PortalGunPortalEntry> p, int i)
        {
            return p[p.Count + i].Exit.Position.x;
        }

        public static int GetExitZ(List<Building_PortalGunPortalEntry> p, int i)
        {
            return p[p.Count + i].Exit.Position.z;
        }

        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            Label loopStartLabel = new Label();
            MethodInfo getPortalsMethod = typeof(PathFinder_FindPath).GetMethod("GetPortals");
            MethodInfo absMethod = typeof(Math).GetMethod("Abs", new Type[] { typeof(Int32) });
            MethodInfo listGetCountMethod = typeof(PathFinder_FindPath).GetMethod("GetPortalsCount");
            MethodInfo entryGetXMethod = typeof(PathFinder_FindPath).GetMethod("GetExitX");
            MethodInfo entryGetZMethod = typeof(PathFinder_FindPath).GetMethod("GetExitZ");

            object cLocal = null;
            CodeInstruction num9Instruction = null;
            CodeInstruction num10Instruction = null;
            object num11Local = null;
            object num12Local = null;
            CodeInstruction num13Instruction = null;
            CodeInstruction num14Instruction = null;
            CodeInstruction xInstruction = null;
            CodeInstruction zInstruction = null;
            object iLocal = null;
            int preLoopIndex = -1;
            int endNormalNeighborsIndex = -1;
            int loopConditionalIndex = -1;
            int octileDistanceIndex = -1;
            for (int i = 0; i < instructionsList.Count; ++i)
            {
                if (instructionsList[i].opcode == OpCodes.Stloc_S &&
                    instructionsList[i + 1].opcode == OpCodes.Ldloca_S && instructionsList[i].operand == instructionsList[i + 1].operand &&
                    instructionsList[i + 2].opcode == OpCodes.Ldfld && instructionsList[i + 2].operand == intVec3XField &&
                    instructionsList[i + 5].opcode == OpCodes.Ldfld && instructionsList[i + 5].operand == intVec3ZField)
                {
                    cLocal = instructionsList[i].operand;
                }
                if (instructionsList[i].opcode == OpCodes.Ldstr &&
                    (instructionsList[i].operand as String).Equals("Neighbor consideration") &&
                    instructionsList[i + 1].opcode == OpCodes.Call &&
                    instructionsList[i + 2].opcode == OpCodes.Ldc_I4_0 &&
                    instructionsList[i + 3].opcode == OpCodes.Stloc_S &&
                    instructionsList[i + 4].opcode == OpCodes.Br)
                {
                    preLoopIndex = i;
                    iLocal = instructionsList[i + 3].operand;
                    loopStartLabel = (Label)instructionsList[i + 4].operand;

                    for (int j = i + 5; j < instructionsList.Count; ++j)
                    {
                        if (instructionsList[j].opcode == OpCodes.Add && instructionsList[j + 1].opcode == OpCodes.Stloc_S)
                        {
                            if (num11Local == null)
                            {
                                num11Local = instructionsList[j + 1].operand;

                            }
                            else
                            {
                                endNormalNeighborsIndex = j + 2;
                                num12Local = instructionsList[j + 1].operand;
                                break;
                            }
                        }
                    }
                }
                if (instructionsList[i].labels.Contains(loopStartLabel) &&
                    instructionsList[i].opcode == OpCodes.Ldloc_S &&
                    instructionsList[i].operand == iLocal)
                {
                    loopConditionalIndex = i;
                }
                if (instructionsList[i].opcode == OpCodes.Call && instructionsList[i].operand == typeof(GenMath).GetMethod("OctileDistance"))
                {
                    octileDistanceIndex = i;
                    if (instructionsList[i-1].opcode == OpCodes.Ldloc_S)
                    {
                        num10Instruction = instructionsList[i - 1];
                    }
                    if (instructionsList[i - 2].opcode == OpCodes.Ldloc_S)
                    {
                        num9Instruction = instructionsList[i - 2];
                    }
                    for (int j = i - 3; j >= 0; --j)
                    {
                        if (instructionsList[j].opcode == OpCodes.Call && instructionsList[j].operand == absMethod && instructionsList[j - 1].opcode == OpCodes.Sub)
                        {
                            if (zInstruction == null)
                            {
                                zInstruction = instructionsList[j - 2];
                                num14Instruction = instructionsList[j - 3];
                            }
                            else
                            {
                                xInstruction = instructionsList[j - 2];
                                num13Instruction = instructionsList[j - 3];
                                break;
                            }
                        }
                    }
                }
            }
            List<string> missingParts = new List<string>();
            if (cLocal == null)
            {
                missingParts.Add("cLocal");
            }
            if (num9Instruction == null)
            {
                missingParts.Add("num9Instruction");
            }
            if (num10Instruction == null)
            {
                missingParts.Add("num10Instruction");
            }
            if (num11Local == null)
            {
                missingParts.Add("num11Local");
            }
            if (num12Local == null)
            {
                missingParts.Add("num12Local");
            }
            if (num13Instruction == null)
            {
                missingParts.Add("num13Instruction");
            }
            if (num14Instruction == null)
            {
                missingParts.Add("num14Instruction");
            }
            if (xInstruction == null)
            {
                missingParts.Add("xInstruction");
            }
            if (zInstruction == null)
            {
                missingParts.Add("zInstruction");
            }
            if (iLocal == null)
            {
                missingParts.Add("iLocal");
            }
            if (preLoopIndex == -1)
            {
                missingParts.Add("preLoopIndex");
            }
            if (endNormalNeighborsIndex == -1)
            {
                missingParts.Add("endNormalNeighborsIndex");
            }
            if (loopConditionalIndex == -1)
            {
                missingParts.Add("loopConditionalIndex");
            }
            if (octileDistanceIndex == -1)
            {
                missingParts.Add("octileDistanceIndex");
            }
            if (missingParts.Count > 0)
            {
                Log.Message("Failed to patch PathFinder.FindPath, couldn't find: " + missingParts.Join());
                for (int i = 0; i < instructionsList.Count; ++i)
                {
                    yield return instructionsList[i];
                }
            }
            else
            {
                LocalBuilder portalList = ilg.DeclareLocal(typeof(List<Building_PortalGunPortalEntry>));
                LocalBuilder portal = ilg.DeclareLocal(typeof(Building_PortalGunPortalEntry));
                Label endNormalNeighbors = ilg.DefineLabel();
                Label normalLoopStart = ilg.DefineLabel();

                instructionsList[endNormalNeighborsIndex].labels.Add(endNormalNeighbors);
                for (int i = 0; i < instructionsList.Count;)
                {
                    CodeInstruction instruction = instructionsList[i];

                    if (i == preLoopIndex)
                    {
                        yield return instructionsList[i];
                        yield return instructionsList[i + 1];
                        // replace load of 0 onto stack.
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, mapField);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, cLocal);
                        yield return new CodeInstruction(OpCodes.Call, getPortalsMethod);
                        yield return new CodeInstruction(OpCodes.Stloc_S, portalList);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, portalList);
                        yield return new CodeInstruction(OpCodes.Call, listGetCountMethod);
                        yield return new CodeInstruction(OpCodes.Neg);
                        yield return instructionsList[i + 3];
                        yield return instructionsList[i + 4];

                        i += 5;
                        continue;
                    }
                    if (i == loopConditionalIndex)
                    {
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Bge, normalLoopStart);

                        /*yield return new CodeInstruction(OpCodes.Ldloc_S, portalList);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, iLocal);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, num11Local);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, num12Local);
                        yield return new CodeInstruction(OpCodes.Call, typeof(PathFinder_FindPath).GetMethod("GetExit"));*/

                        yield return new CodeInstruction(OpCodes.Ldloc_S, portalList);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, iLocal);
                        yield return new CodeInstruction(OpCodes.Call, entryGetXMethod);
                        yield return new CodeInstruction(OpCodes.Stloc_S, num11Local);

                        yield return new CodeInstruction(OpCodes.Ldloc_S, portalList);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, iLocal);
                        yield return new CodeInstruction(OpCodes.Call, entryGetZMethod);
                        yield return new CodeInstruction(OpCodes.Stloc_S, num12Local);
                        yield return new CodeInstruction(OpCodes.Br, endNormalNeighbors);

                        CodeInstruction newLoadI = new CodeInstruction(OpCodes.Ldloc_S, iLocal);
                        newLoadI.labels.Add(normalLoopStart);
                        yield return newLoadI;
                        i++;
                        continue;

                    }
                    if (i == octileDistanceIndex)
                    {
                        yield return instruction;
                        ++i;

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, mapField);
                        yield return num13Instruction;
                        yield return num14Instruction;
                        yield return xInstruction;
                        yield return zInstruction;
                        yield return num9Instruction;
                        yield return num10Instruction;
                        yield return new CodeInstruction(OpCodes.Call, typeof(Utilities).GetMethod("CalculateOctileDistance"));
                        continue;
                    }

                    yield return instruction;
                    ++i;
                }
            }
        }
    }
}
