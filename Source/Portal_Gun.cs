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

        }
    }
}