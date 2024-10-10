using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Modding.Logger;

namespace BetterTotems;

[UsedImplicitly]
public class BetterTotems : Modding.Mod
{
    private static GameObject betterOrb = null;

    public override void Initialize()
    {
        BetterLog("!Initialize");

        On.PlayMakerFSM.Start += ChangeTotems;

        //On.GameCameras.Start += AddMasks;

        DoAddMasks(GameCameras.instance);

        BetterLog("~Initialize");
    }

    private void AddMasks(On.GameCameras.orig_Start orig, GameCameras self)
    {
        orig(self);
    }

    private void DoAddMasks(GameCameras self)
    {
        BetterLog("!DoAddMasks");
        GameObject health1Go = self.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(3).gameObject;

        int totalMaskAmount = 28 * 13;
        for (int i = 12; i < totalMaskAmount + 1; i++)
        {
            GameObject healthGo = Object.Instantiate(health1Go, health1Go.transform.parent);
            SetHealthPositionAndFsm(healthGo, i);
        }
        SetHealthPosition(self.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(4).gameObject, totalMaskAmount + 1);
        BetterLog("!DoAddMasks");
    }

    private void SetHealthPositionAndFsm(GameObject ob, int num)
    {
        BetterLog("!SetHealthPositionAndFsm");
        ob.name = $"Health {num}";
        PlayMakerFSM healthFsm = ob.LocateMyFSM("health_display");
        FsmVariables healthFsmVars = healthFsm.FsmVariables;
        healthFsmVars.GetFsmInt("Health Number").Value = num;

        SetHealthPosition(ob, num);
        BetterLog("!SetHealthPositionAndFsm");
    }

    private void SetHealthPosition(GameObject ob, int num)
    {
        BetterLog("!SetHealthPosition");
        int tmp = num;
        int row = 0;
        while (tmp > 28)
        {
            row++;
            tmp -= 28;
        }
        //int row = (int) (((float) num - 1f) / 29f);

        float xPos = -10.32f + (0.94f * (tmp - 1)) + ((row % 2 == 0) ? 0 : (0.94f / 2f));
        float yPos = 7.7f - (0.8f * row);

        ob.transform.localPosition = new Vector3(xPos, yPos, -2);
        BetterLog("!SetHealthPosition");
    }

    private void ChangeTotems(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
    {
        orig(self);

        if (self.FsmName != "soul_totem")
            return;

        BetterLog("!ChangeTotems");

        if (self.FsmStates[0].Fsm == null)
        {
            BetterLog("Preprocessing");
            self.Preprocess();
        }

        typeof(PlayMakerFSM).GetField("fsmTemplate", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, null);
        FlingObjectsFromGlobalPool fofgpAction = (FlingObjectsFromGlobalPool) self.FsmStates.First(x => x.Name.Equals("Hit")).Actions[8];
        if (betterOrb == null)
        {
            BetterLog("!betterOrb");
            //betterOrb = new GameObject("Better Soul Orb", typeof(SpriteRenderer), typeof(Rigidbody2D),
            //    typeof(AudioSource), typeof(BetterSoulOrb));
            betterOrb = Object.Instantiate(fofgpAction.gameObject.Value);
            betterOrb.name = "Better Orb";
            betterOrb.SetActive(false);

            SoulOrb so = betterOrb.GetComponent<SoulOrb>();
            BetterSoulOrb bso = betterOrb.AddComponent<BetterSoulOrb>();

            bso.soulOrbCollectSounds = so.soulOrbCollectSounds;
            bso.getParticles = so.getParticles;
            bso.awardSoul = so.awardSoul;
            bso.dontRecycle = so.dontRecycle;
            bso.stretchFactor = so.stretchFactor;
            bso.stretchMinY = so.stretchMinY;
            bso.stretchMaxX = so.stretchMaxX;
            bso.scaleModifier = so.scaleModifier;
            bso.scaleModifierMin = so.scaleModifierMin;
            bso.scaleModifierMax = so.scaleModifierMax;
            bso.SoulGotten = 200;

            Object.DestroyImmediate(betterOrb.GetComponent<SoulOrb>());
            Object.DontDestroyOnLoad(betterOrb);

            BetterLog("~betterOrb");
        }

        fofgpAction.gameObject.Value = betterOrb;
        fofgpAction.spawnMin = 1;
        fofgpAction.spawnMax = 1;

        BetterLog("~ChangeTotems");
    }

    private static void BetterLog(string message)
    {
        Logger.Log($"[BetterTotem] - {message}");
        Debug.Log($"[BetterTotem] - {message}");
    }
    private static void BetterLog(object message)
    {
        BetterLog($"{message}");
    }
}