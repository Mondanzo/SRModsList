using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using PSLib;

public class SynthRidersModList : MelonMod {

    Transform btn;
    Transform home_buttons;
    Transform home;
    GameObject btn_mods;

    GameObject mods_panel;

    bool isMenu = false;

    public void OpenModList(object sender, VRTK.InteractableObjectEventArgs e)
    {
        home.gameObject.SetActive(false);
        for (int i = 0; i < mods_panel.transform.parent.childCount; i++)
        {
            mods_panel.transform.parent.GetChild(i).gameObject.SetActive(false);
        }

        if (mods_panel) mods_panel.SetActive(true);
        mods_panel.SetActive(true);
        mods_panel.transform.parent.parent.parent.gameObject.SetActive(true);
    }

    public void CloseModList()
    {
        if(mods_panel && mods_panel.activeSelf)
        {
            mods_panel.SetActive(false);
            mods_panel.transform.parent.parent.parent.gameObject.SetActive(false);
            mods_panel.transform.parent.GetChild(0).gameObject.SetActive(true);
            mods_panel.transform.parent.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void InjectModButton()
    {
        btn = home_buttons.Find("Credits");
        btn_mods = GameObject.Instantiate(btn.gameObject);
        btn_mods.transform.name = "Mods";
        btn_mods.transform.SetParent(home_buttons);
        
        btn_mods.transform.GetChild(0).name = "FocusButton - Mods";
        
        btn_mods.GetComponentInChildren<TMPro.TMP_Text>().text = "mods";
        VRTK.UnityEventHelper.VRTK_InteractableObject_UnityEvents vrtk = btn_mods.GetComponentInChildren<VRTK.UnityEventHelper.VRTK_InteractableObject_UnityEvents>();
        for(int i = vrtk.OnUse.GetPersistentEventCount() - 1; i >= 0; i--) {
                    vrtk.OnUse.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
        }
        vrtk.OnUse.AddListener(OpenModList);
    }

    public bool check_main_menu_scene(string scene_name)
    {
        try
        {
            isMenu = Game_InfoProvider.MenuScene == int.Parse(scene_name.Split('.')[0]);
            return isMenu;
        } catch (Exception e)
        {
            return false;
        }
    }

    public override void OnUpdate()
    {
        if (isMenu && btn_mods != null) { 
            btn_mods.transform.rotation = btn.rotation;
            btn_mods.transform.position = btn.position;
            btn_mods.transform.localPosition = btn.localPosition;
            btn_mods.transform.localScale = btn.localScale;
            btn_mods.transform.GetChild(0).transform.position = btn.GetChild(0).transform.position;
            btn_mods.transform.GetChild(0).transform.localPosition = btn.GetChild(0).transform.localPosition;
            btn_mods.transform.GetChild(0).transform.localPosition += new Vector3(0, -0.16f, 0);
        }
    }

    public void InjectModMenu()
    {
        GameObject obj = null;
        foreach(var go in GameObject.Find("Z-Wrap").transform.GetComponentsInChildren<Game_PauseMenuPanel>(true))
        {
            if(go.gameObject.name == "[Interface Panel]")
            {
                obj = go.gameObject;
            }
        }
        if (obj == null) return;
        mods_panel = GameObject.Instantiate(obj, obj.transform.position, obj.transform.rotation, obj.transform.parent);
        mods_panel.name = "[Mods Panel]";
        //mods_panel.transform.Find("[BG Layer]").localScale = new Vector3(0.127f, 0.159f, 0.169f);
        GameObject t_mod_title = null;
        GameObject t_mod_label = null;
        foreach(var go in mods_panel.GetComponentsInChildren<UnityEngine.RectTransform>())
        {
            if(t_mod_title == null)
            {
                if(go.name.StartsWith("Setting Header"))
                {
                    t_mod_title = go.gameObject;
                }
            }

            if (t_mod_label == null)
            {
                if (go.name.StartsWith("Setting Item"))
                {
                    t_mod_label = go.gameObject;
                }
            }
        }
        MelonLogger.Msg("Trying to generate Mod Entries...");
        if (t_mod_title == null) return;
        if (t_mod_label == null) return;

        var mods = t_mod_label.transform.parent;

        int childrens = mods.childCount;

        GameObject mod_title = GameObject.Instantiate(t_mod_title, t_mod_title.transform.position, t_mod_title.transform.rotation, mods);
        mod_title.name = "Heading Mods";
        mod_title.GetComponentInChildren<Synth.Utils.LocalizationHelper>().enabled = false;
        mod_title.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText("Mods");

        foreach (var mod in MelonHandler.Mods)
        {
            GameObject mod_label = GameObject.Instantiate(t_mod_label, t_mod_label.transform.position, t_mod_label.transform.rotation, mods);
            mod_label.name = "Mod - " + mod.Info.Name;
            mod_label.GetComponentInChildren<Synth.Utils.LocalizationHelper>().enabled = false;
            mod_label.transform.Find("Value Area").gameObject.SetActive(false);
            mod_label.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(mod.Info.Name + " by " + mod.Info.Author + " - " + mod.Info.Version);
            mod_label.GetComponent<VRTK.UnityEventHelper.VRTK_InteractableObject_UnityEvents>().enabled = false;
        }

        for (int i = childrens - 1; i >= 0; i--)
        {
            GameObject.Destroy(mods.GetChild(i).gameObject);
        }

        mods_panel.transform.parent.parent.parent.Find("Nav Bar").Find("Scale Wrap").Find("Button Wrap - Full").Find("NavBarButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseModList);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (check_main_menu_scene(sceneName))
        {
            home = GameObject.Find("Z-Wrap").transform.Find("Home");
            home_buttons = GameObject.Find("Z-Wrap").transform.Find("Home/wrap");
            InjectModButton();
            InjectModMenu();
        }
    }
}