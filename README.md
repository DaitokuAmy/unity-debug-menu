# unity-debug-menu
![debug_item_sample](https://github.com/DaitokuAmy/unity-debug-menu/assets/6957962/9b614cdc-27d8-4f89-8f18-901e9cd6d105)
![debug_window_sample](https://github.com/DaitokuAmy/unity-debug-menu/assets/6957962/a380bc37-e4cf-4f1b-a96e-f20c075add74)

## 概要
#### 特徴
* UnityのIMGUIを使った簡易的なDebugMenu機能
## セットアップ
#### インストール
1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下を入力してインストール
   * https://github.com/DaitokuAmy/unity-debug-menu.git?path=/Packages/com.daitokuamy.unitydebugmenu
   ![image](https://user-images.githubusercontent.com/6957962/209446846-c9b35922-d8cb-4ba3-961b-52a81515c808.png)  
あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記します。
```json
{
    "dependencies": {
        "com.daitokuamy.unitydebugmenu": "https://github.com/DaitokuAmy/unity-debug-menu.git?path=/Packages/com.daitokuamy.unitydebugmenu"
    }
}
```
バージョンを指定したい場合には以下のように記述します。  
https://github.com/DaitokuAmy/unity-debug-menu.git?path=/Packages/com.daitokuamy.unitydebugmenu#1.0.0

## 使い方
#### Configファイルの作成
![image](https://github.com/DaitokuAmy/unity-debug-menu/assets/6957962/eea79f06-8531-44c8-a0fd-b163637c4731)
1. ProjectWindowにて、Create > Unity Debug Menu > Config を選択して、Resourcesフォルダ直下にファイルを作成する

### DebugMenuの登録/登録解除
* ボタンを追加し、処理を実行する場合
```csharp
// Test/Sample階層にボタンを作成し、ログを出力する
DebugMenu.AddItem("Test/Sample", _ => {
    Debug.Log("Execute Sample");
});

// Test以下の登録したボタンを削除
DebugMenu.RemoveItem("Test");
```

* ウィンドウを追加する場合
```csharp
// Test/SampleWindow階層にWindowを開くボタンを作成し、WindowのGUIを登録する
DebugMenu.AddWindowItem("Test/SampleWindow", _ => {
    GUILayout.Label("Test");
    if (GUILayout.Button("Execute")) {
        Debug.Log("Execute Sample Button");
    }

    // DebugMenuUtilクラスにLabel/ValueタイプのGUIユーティリティがある
    _testEnum = DebugMenuUtil.EnumArrowOrderField("TestEnum", _testEnum);
});

// Test以下の登録したボタンを削除(Itemの時と同じ)
DebugMenu.RemoveItem("Test");
```

### DebugMenuを表示する方法
* UnityEditorの場合
  - GameViewを右クリック

* 実機の場合
  - デフォルト指定の場合、2本指タッチで3秒長押し or 3本指タッチで1秒長押し
