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
