using RimWorld;
using HugsLib;
using HugsLib.Utils;
using HugsLib.Settings;
using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using Portal_Gun.Buildings;

namespace Portal_Gun
{
    public class Portal_Gun : ModBase
    {
        public static Portal_Gun Instance { get; private set; }
        public Dictionary<Map, PortalManager> PortalManagers = new Dictionary<Map, PortalManager>();


        internal static SettingHandle<float> portalPowerDrawMultiplier;
        internal static SettingHandle<bool> solarFlareInterferenceEnabled;

        public override string ModIdentifier
        {
            get { return "Portal_Gun"; }
        }

        public Portal_Gun()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();

            portalPowerDrawMultiplier = Settings.GetHandle<float>("portalPowerDrawMultiplier", "PA_PortalPowerDrawMultiplier_Title".Translate(), "PA_PortalPowerDrawMultiplier_Description".Translate(), 1.0f, Validators.FloatRangeValidator(0.0f, 4.0f));
            solarFlareInterferenceEnabled = Settings.GetHandle<bool>("solarFlareInterferenceEnabled", "PA_SolarFlareInterferenceEnabled_Title".Translate(), "PA_SolarFlareInterferenceEnabled_Description".Translate(), true);

        }
    }
}