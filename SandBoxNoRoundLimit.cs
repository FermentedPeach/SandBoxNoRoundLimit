using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.BloonMenu;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppTMPro;
using MelonLoader;
using SandBoxNoRoundLimit;
using System;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(SandBoxNoRoundLimit.SandBoxNoRoundLimit), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace SandBoxNoRoundLimit;

public class SandBoxNoRoundLimit : BloonsTD6Mod
{

    [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Initialise))]
    internal static class BloonMenu_PostInitialised
    {
        [HarmonyPostfix]
        private static void PostFix(BloonMenu __instance)
        {
            Button startbtn = null;
            foreach (var item in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.InstanceID))
            {
                if (item.name.Equals("Go"))
                {
                    startbtn = item;
                    break;
                }
            }

            var newBtnClickEvt = new Button.ButtonClickedEvent();
            newBtnClickEvt.AddListener(() => {
                InGame.instance.bridge.SetRound(Int32.Parse(__instance.roundDetails.m_Text) - 1);
                InGame.instance.bridge.ResetRound();
                InGame.instance.bridge.ClearBloons();
                InGame.instance.bridge.StartRound();
            });

            startbtn.onClick = newBtnClickEvt;

            __instance.roundDetails.characterLimit = 4; //allow up to round 9999 (no one will ever get here lol)
        }
    }

    [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.OnRoundValueChanged), [typeof(string)])]
    internal static class BloonMenu_RoundValueChanged
    {
        [HarmonyPostfix]
        private static void PostFix(BloonMenu __instance, string newValue)
        {
            if (!newValue.Equals(__instance.roundDetails.m_Text))
            {
                InGame.instance.bridge.SetRound(Int32.Parse(newValue) - 1);
                __instance.SetRoundText(Int32.Parse(newValue));
            }
        }
    }

    //Fix the cash and health popups to give up to 999999999 money/health
    [HarmonyPatch(typeof(Popup), nameof(Popup.ShowPopup))]
    internal static class Popup_ShowPopup
    {
        [HarmonyPrefix]
        private static void Prefix(Popup __instance)
        {

            if (__instance.gameObject.name.Contains("SetValuePopup"))
            {
                __instance.GetComponentInChildren<TMP_InputField>().characterLimit = 9;
            }
        }
    }

    public override void OnApplicationStart()
    {
        ModHelper.Msg<SandBoxNoRoundLimit>("SandBoxNoRoundLimit loaded!");
    }
}