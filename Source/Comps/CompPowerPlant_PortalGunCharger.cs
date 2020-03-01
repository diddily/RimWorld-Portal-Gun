#if VERSION_1_0
using Harmony;
#else
using HarmonyLib;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Portal_Gun.Buildings;
using Portal_Gun.Items;

namespace Portal_Gun.Comps
{
	public class CompPowerPlant_PortalGunCharger : CompPowerPlant
	{
		public CompPowerPlantProperties_PortalGunCharger PG_Props => props as CompPowerPlantProperties_PortalGunCharger;
		private CompFacility compFacility => parent.GetComp<CompFacility>();
		private Sustainer chargeSound;

		private static FieldInfo linkedBuildingsField = AccessTools.Field(typeof(CompFacility), "linkedBuildings");

		protected IEnumerable<Item_PortalGun> LinkedPortalGuns
		{
			get
			{
				return (linkedBuildingsField.GetValue(compFacility) as List<Thing>).OfType<Item_PortalGun>();
			}
		}

		public bool IsCharging
		{
			get
			{
				return LinkedPortalGuns.Count() > 0;//DesiredPowerOutput < -Props.basePowerConsumption;
			}
		}

		protected override float DesiredPowerOutput
		{
			get
			{
				return base.DesiredPowerOutput - LinkedPortalGuns.Sum(ipg => ipg.PowerExternal);
			}
		}

		public override void PostDeSpawn(Map map)
		{
			if (chargeSound != null)
			{
				chargeSound.End();
				chargeSound = null;
			}
			base.PostDeSpawn(map);
		}

		public override void CompTick()
		{
			bool wantsActive = IsCharging;
			bool isActive = chargeSound != null;
			if (wantsActive != isActive)
			{
				if (wantsActive)
				{
					chargeSound = PG_DefOf.PG_ChargeLoop.TrySpawnSustainer(parent);
				}
				else
				{
					chargeSound.End();
					chargeSound = null;
				}
			}
			base.CompTick();
		}
	}
}
