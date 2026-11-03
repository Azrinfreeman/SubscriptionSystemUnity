using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-time, explicit migration of the existing scenes. Finished layouts are serialized,
// editable scene objects; this tool never runs automatically or at runtime.
public static class GameUIRevampTools
{
    static GameUITheme theme;
    static Sprite rounded;
    static TMP_FontAsset font;
    static readonly string[] Scenes = { "LogMasuk", "LamanUtama" };
    static T Add<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    static RectTransform Rect(Transform t, float x, float y, float w, float h)
    {
        var r = (RectTransform)t; r.anchorMin = r.anchorMax = new Vector2(x, y);
        r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = Vector2.zero;
        r.sizeDelta = new Vector2(w, h); r.localScale = Vector3.one; return r;
    }
    static void Fill(Transform t, float inset = 0)
    {
        var r = (RectTransform)t; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.one * inset; r.offsetMax = Vector2.one * -inset; r.localScale = Vector3.one;
    }
    static void Bind(Graphic g, GameUIThemeBinding.Role role)
    {
        var b = Add<GameUIThemeBinding>(g.gameObject); b.theme = theme; b.role = role; b.Apply();
    }
    static Image Surface(Transform t, GameUIThemeBinding.Role role)
    {
        var i = Add<Image>(t.gameObject); i.enabled = true; i.sprite = rounded; i.type = Image.Type.Sliced;
        Bind(i, role); return i;
    }
    static TextMeshProUGUI Label(Transform parent, string name, string text, int size)
    {
        var t = parent.Find(name); if (t == null) { t = new GameObject(name, typeof(RectTransform)).transform; t.SetParent(parent, false); }
        var label = Add<TextMeshProUGUI>(t.gameObject); label.font = font; label.text = text;
        label.fontSize = size; label.enableAutoSizing = false; label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false; Bind(label, GameUIThemeBinding.Role.Ink); return label;
    }
    static void Button(Button b, string text, bool primary = true)
    {
        foreach (var child in b.transform.Cast<Transform>().ToArray()) child.gameObject.SetActive(false);
        var face = b.transform.Find("Face");
        if (face == null) { face = new GameObject("Face", typeof(RectTransform)).transform; face.SetParent(b.transform, false); }
        face.gameObject.SetActive(true); Fill(face);
        var image = Surface(face, primary ? GameUIThemeBinding.Role.Primary : GameUIThemeBinding.Role.Field);
        image.raycastTarget = false;
        var hit = Add<Image>(b.gameObject); hit.sprite = rounded; hit.type = Image.Type.Sliced; hit.color = new Color(1, 1, 1, .01f); hit.raycastTarget = true;
        var label = Label(face, "Label", text, 26); Fill(label.transform, 8);
        Bind(label, primary ? GameUIThemeBinding.Role.Surface : GameUIThemeBinding.Role.Ink);
        b.targetGraphic = image; b.transition = Selectable.Transition.ColorTint;
        var colors = b.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1.16f, 1.16f, 1.16f);
        colors.selectedColor = new Color(1.25f, 1.25f, 1.25f); colors.pressedColor = new Color(.78f, .78f, .78f);
        colors.disabledColor = new Color(.6f, .6f, .6f, .65f); colors.fadeDuration = .08f; b.colors = colors;
        var outline = Add<Outline>(face.gameObject); outline.effectColor = theme.highlight; outline.effectDistance = new Vector2(2, -3);
        var feedback = Add<GameUIButtonFeedback>(b.gameObject); feedback.theme = theme; feedback.visual = (RectTransform)face;
    }
    static void Container(Transform t)
    {
        foreach (var l in t.GetComponents<LayoutGroup>()) UnityEngine.Object.DestroyImmediate(l);
        foreach (var f in t.GetComponents<ContentSizeFitter>()) UnityEngine.Object.DestroyImmediate(f);
        Fill(t); var g = t.GetComponent<Graphic>(); if (g != null) { g.color = Color.clear; g.raycastTarget = false; }
    }
    static void Field(TMP_InputField input, string label, float y)
    {
        Rect(input.transform, .5f, y, 560, 64);
        Surface(input.transform, GameUIThemeBinding.Role.Field);
        foreach (Transform child in input.transform)
            if (child.name != "Text Area" && child.name != "btnVerification" && child.name != "no" && child.name != "yes") child.gameObject.SetActive(false);
        Fill(input.textViewport, 16);
        input.textComponent.fontSize = 26; input.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        Bind(input.textComponent, GameUIThemeBinding.Role.Ink);
        if (input.placeholder is TMP_Text placeholder) { placeholder.text = label; placeholder.fontSize = 25; placeholder.fontStyle = FontStyles.Normal; Bind(placeholder, GameUIThemeBinding.Role.Ink); }
        var title = Label(input.transform, "FieldLabel", label, 22); title.gameObject.SetActive(true);
        title.alignment = TextAlignmentOptions.MidlineLeft; Rect(title.transform, .5f, 1, 560, 28).anchoredPosition = new Vector2(0, 22);
        if (input.readOnly) { var nav = input.navigation; nav.mode = Navigation.Mode.None; input.navigation = nav; }
    }
    static void Panel(Transform root, string title, bool form)
    {
        var animator = root.GetComponent<Animator>(); if (animator != null) animator.enabled = false;
        Fill(root); root.localScale = Vector3.one;
        var dim = root.Find("Image"); if (dim != null) { Fill(dim); var i = dim.GetComponent<Image>(); i.sprite = null; i.color = new Color(.08f, .17f, .22f, .75f); i.raycastTarget = true; }
        var panel = root.Find("Panel");
        foreach(var layout in panel.GetComponents<LayoutGroup>()) UnityEngine.Object.DestroyImmediate(layout);
        Rect(panel, .5f, .5f, 680, form ? 920 : 800);
        Surface(panel, GameUIThemeBinding.Role.Surface).raycastTarget = true;
        var flow = Add<GameUIPanelFlow>(panel.gameObject); flow.theme = theme; flow.panelRoot = root.gameObject;
        var bg = panel.Find("bg"); Container(bg);
        var art = bg.Find("1"); if (art != null) art.gameObject.SetActive(false);
        var content = bg.Find("2"); if (content != null) Container(content);
        var top = panel.Find("Top"); if (top != null) { Rect(top, .5f, .93f, 600, 72); var g=top.GetComponent<Graphic>(); if(g!=null){g.color=Color.clear;g.raycastTarget=false;} var label=top.GetComponentInChildren<TMP_Text>(); label.text=title; label.fontSize=38; Fill(label.transform); }
        if (content != null)
        {
            var back = content.Find("KembaliBtn"); if (back != null) { Rect(back, .5f, .065f, 560, 64); var button=back.GetComponentInChildren<Button>(true); Fill(button.transform); Button(button,"Kembali",false); flow.backButton=button; }
            var output = content.Find("outputText"); if(output!=null) { Rect(output,.5f,.19f,560,74); var text=output.GetComponent<TMP_Text>();text.fontSize=22;Bind(text,GameUIThemeBinding.Role.Error); }
        }
    }
    static void Form(Transform root, bool register)
    {
        Panel(root, register ? "Daftar akaun" : "Log masuk", true);
        var content=root.Find("Panel/bg/2");
        if(register) Field(content.Find("Name").GetComponent<TMP_InputField>(),"Nama",.79f);
        Field(content.Find("Email").GetComponent<TMP_InputField>(),"Email",register?.65f:.73f);
        if(register) Field(content.Find("NoTel").GetComponent<TMP_InputField>(),"Nombor telefon",.51f);
        Field(content.Find("Password").GetComponent<TMP_InputField>(),"Kata laluan",register?.37f:.54f);
        foreach(var warning in new[]{"EmailWarning","PhoneWarning"}) { var t=content.Find(warning); if(t!=null) { Rect(t,.5f,warning=="EmailWarning"?(register?.59f:.67f):.45f,560,30); var tx=t.GetComponentInChildren<TMP_Text>(true);Fill(tx.transform);tx.fontSize=20;Bind(tx,GameUIThemeBinding.Role.Error); } }
        var action=content.Find(register?"DaftarBtn":"LogBtn"); Rect(action,.5f,.26f,560,68);
        var submit=action.GetComponentInChildren<Button>(true); Fill(submit.transform); Button(submit,register?"Daftar akaun":"Log masuk");
        // Existing request controllers keep owning notices. Make them an inline summary,
        // leaving Back and retry reachable instead of using a blocking animated overlay.
        var notice=root.Find("Notice"); if(notice!=null) {
            foreach(var a in notice.GetComponentsInChildren<Animator>(true))a.enabled=false;
            Rect(notice,.5f,.5f,560,70); notice.SetParent(content,false);Rect(notice,.5f,.16f,560,70);
            foreach(var i in notice.GetComponentsInChildren<Image>(true)){i.enabled=false;i.raycastTarget=false;}
            foreach(var t in notice.GetComponentsInChildren<RectTransform>(true))if(t!=notice)Fill(t);
            foreach(var t in notice.GetComponentsInChildren<TMP_Text>(true)){t.fontSize=22;Bind(t,GameUIThemeBinding.Role.Error);}
            notice.gameObject.SetActive(false);
        }
    }
    static void Details(Transform root, bool subscription)
    {
        Panel(root,subscription?"Langganan":"Profil pemain",false);
        var content=root.Find("Panel/bg/2");
        Field(content.Find("Name").GetComponent<TMP_InputField>(),subscription?"Pelan":"Nama",.78f);
        Field(content.Find("Email").GetComponent<TMP_InputField>(),subscription?"Tarikh mula":"Email",.63f);
        Field(content.Find("NoTel").GetComponent<TMP_InputField>(),subscription?"Tamat akses":"Nombor telefon",.48f);
        var status=content.Find(subscription?"Status":"Email (3)");
        Field(status.GetComponent<TMP_InputField>(),subscription?"Status akses":"Pengesahan email",.32f);
        foreach(var icon in new[]{"no","yes"}) {var t=status.Find(icon);if(t!=null)Rect(t,.94f,.5f,28,28);}
        var action=subscription?content.Find("Manage"):status.Find("btnVerification");
        action.SetParent(content,false);Rect(action,.5f,.20f,560,56);Button(action.GetComponent<Button>(),subscription?"Urus langganan":"Hantar email pengesahan");
        var hint=action.Find("hint");if(hint!=null)Rect(hint,.5f,-.3f,560,28);
    }
    static void Prompt(Transform root, bool logout)
    {
        var a=root.GetComponent<Animator>();if(a!=null)a.enabled=false;
        var panel=root.Find("Panel");Rect(panel,.5f,.5f,680,480);Surface(panel,GameUIThemeBinding.Role.Surface);
        var bg=panel.Find("bg"); if(bg==null)bg=panel;else Container(bg);
        var message=bg.GetComponentsInChildren<TMP_Text>(true).First();Rect(message.transform,.5f,.72f,560,150);message.fontSize=30;
        message.text=logout?"Log keluar dari akaun ini?":"Akaun ini sedang digunakan pada peranti lain.";
        var device=bg.Find("peranti");if(device!=null)Rect(device,.5f,.48f,560,60);
        var buttons=bg.GetComponentsInChildren<Button>(true);for(int i=0;i<buttons.Length;i++){Rect(buttons[i].transform,.5f,i==0?.30f:.13f,560,64);Button(buttons[i],i==0?"Log keluar":"Kembali",i==0);}
        var flow=Add<GameUIPanelFlow>(panel.gameObject);flow.theme=theme;flow.panelRoot=root.gameObject;flow.backButton=buttons.Last();
    }
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        if(File.ReadAllText("Assets/Scenes/LogMasuk.unity").Contains("MotionPreference"))throw new InvalidOperationException("Migration already applied. Edit scenes and theme directly.");
        theme=AssetDatabase.LoadAssetAtPath<GameUITheme>("Assets/UI/Settings/DefaultGameUITheme.asset");
        if(theme==null){theme=ScriptableObject.CreateInstance<GameUITheme>();AssetDatabase.CreateAsset(theme,"Assets/UI/Settings/DefaultGameUITheme.asset");}
        rounded=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset").First()));
        foreach(var sceneName in Scenes)
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");
            var canvas=UnityEngine.Object.FindObjectOfType<Canvas>();var root=canvas.transform;
            var background=root.Find("background").GetComponent<Image>();background.color=Color.white;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            foreach(var t in root.GetComponentsInChildren<TMP_Text>(true)){t.font=font;Bind(t,GameUIThemeBinding.Role.Ink);t.raycastTarget=false;}
            foreach(var i in root.GetComponentsInChildren<Image>(true)) if(i.GetComponent<Selectable>()==null)i.raycastTarget=false;
            if(sceneName=="LogMasuk")
            {
                var title=root.Find("title").GetComponent<TMP_Text>();title.text="Dunia Anda";title.fontSize=76;Rect(title.transform,.5f,.77f,900,120);
                var intro=Label(root,"Welcome","Selamat datang kembali",30);Rect(intro.transform,.5f,.66f,650,60);
                foreach(var name in new[]{"UtkSignIn","UtkSignUp"}) {var t=root.Find(name);Container(t);var helper=t.Find("Text (TMP) (1)");helper.gameObject.SetActive(false);var b=t.GetComponentInChildren<Button>();Rect(b.transform,.5f,name=="UtkSignIn"?.48f:.36f,480,80);Button(b,name=="UtkSignIn"?"Log masuk":"Daftar akaun",name=="UtkSignIn");}
                Form(root.Find("SignIn"),false);Form(root.Find("SignUp"),true);Prompt(root.Find("LogoutPanel"),false);
            }
            else
            {
                Container(root.Find("Top"));var name=root.Find("Top").GetComponentInChildren<TMP_Text>();Rect(name.transform,.5f,.80f,900,90);name.fontSize=52;
                var title=root.Find("Text (TMP) (1)").GetComponent<TMP_Text>();title.text="Ruang pemain";title.fontSize=34;Rect(title.transform,.5f,.69f,700,70);
                var bottom=root.Find("bottom");Container(bottom);foreach(Transform t in bottom)Container(t);
                string[] names={"Profile","SubscriptionDetail","Cart","TukarPlan","Logout"};string[] labels={"Profil pemain","Langganan saya","Langgan","Tukar pelan","Log keluar"};
                for(int j=0;j<names.Length;j++){var b=bottom.GetComponentsInChildren<Button>(true).First(x=>x.name==names[j]);Rect(b.transform,.5f,.55f-j*.087f,480,72);Button(b,labels[j],j<3);}
                Details(root.Find("Profile"),false);Details(root.Find("SubscriptionDetails"),true);Prompt(root.Find("Prompt"),true);
            }
            var preference=new GameObject("MotionPreference",typeof(RectTransform),typeof(Image),typeof(Button));preference.transform.SetParent(root,false);Rect(preference.transform,.5f,.055f,330,52);var pb=preference.GetComponent<Button>();Button(pb,"Animasi: penuh",false);
            var prefs=preference.AddComponent<GameUIPreferences>();prefs.label=preference.transform.Find("Face/Label").GetComponent<TMP_Text>();UnityEventTools.AddPersistentListener(pb.onClick,prefs.ToggleMotion);
            // Keep decoration below all existing modal roots.
            preference.transform.SetSiblingIndex(sceneName=="LogMasuk"?5:4);
            foreach(var panel in root.GetComponentsInChildren<GameUIPanelFlow>(true))panel.panelRoot.SetActive(false);
            // Safe area contains functional UI; artwork continues to bleed to the edges.
            var safe=new GameObject("SafeArea",typeof(RectTransform),typeof(GameUISafeArea));safe.transform.SetParent(root,false);Fill(safe.transform);
            foreach(var child in root.Cast<Transform>().ToArray())if(child!=safe.transform && child.name!="background")child.SetParent(safe.transform,false);
            for(int j=0;j<3;j++) {var spark=Label(root,"SkySpark"+j,"*",42);Bind(spark,GameUIThemeBinding.Role.Highlight);Rect(spark.transform,.15f+j*.34f,.70f-j*.20f,50,50);spark.gameObject.AddComponent<GameUIAmbient>().period=5+j;spark.transform.SetSiblingIndex(1);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();return "Both existing scenes restyled and saved.";
    }
    public static string Capture(string path, int width=1920, int height=1080)
    {
        var camera=Camera.main;var canvas=UnityEngine.Object.FindObjectOfType<Canvas>();
        var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;var distance=canvas.planeDistance;
        var rt=new RenderTexture(width,height,24);rt.Create();var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
        var image=new Texture2D(width,height,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());}
        finally{canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;camera.targetTexture=oldTarget;RenderTexture.active=oldActive;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);Canvas.ForceUpdateCanvases();}
        return path;
    }
}
