# unity-debug-menu
![debug_item_sample](https://github.com/DaitokuAmy/unity-debug-menu/assets/6957962/b70ecfea-2b89-4ec5-8d77-9212ac19451d)
![debug_window_sample](https://github.com/DaitokuAmy/unity-debug-menu/assets/6957962/d7db79ce-1220-416f-9f03-63e5e75886e7)
![debug_config_sample](https://github.com/DaitokuAmy/unity-debug-menu/assets/6957962/a5d14699-1077-47ab-aecb-52cef0b7e90f)

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
![image](https://github.com/DaitokuAmy/unity-debug-menu/assets/6957962/5322560e-4369-4d4d-875a-7c91922b26b2)
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
});

// Test以下の登録したボタンを削除(Itemの時と同じ)
DebugMenu.RemoveItem("Test");
```
