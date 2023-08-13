using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityDebugMenu {
    /// <summary>
    /// DebugMenuUtil
    /// </summary>
    public static class DebugMenuUtil {
        /// <summary>
        /// ラベルフィールドの幅を設定するスコープ
        /// </summary>
        public class LabelFieldWidthScope : IDisposable {
            private readonly float _prevWidth;

            public LabelFieldWidthScope(float width) {
                _prevWidth = LabelFieldWidth;
                LabelFieldWidth = width;
            }

            public void Dispose() {
                LabelFieldWidth = _prevWidth;
            }
        }

        /// <summary>
        /// 変更監視
        /// </summary>
        public class ChangeCheckScope : IDisposable {
            private bool _prevChanged;

            public bool Changed => !_prevChanged && GUI.changed;

            public ChangeCheckScope() {
                _prevChanged = GUI.changed;
            }

            public void Dispose() {
                _prevChanged = true;
            }
        }

        // フィルター
        private static readonly Dictionary<string, string> Filters = new();

        /// <summary>ラベルフィールドの幅</summary>
        public static float LabelFieldWidth { get; set; } = 150.0f;
        /// <summary>ラベルフィールドのGUILayoutOption</summary>
        public static List<GUILayoutOption> LabelFieldGuiLayoutOptions { get; set; } = new();

        /// <summary>
        /// ラベルと値のフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="valueFieldFunc">右辺に表示する値フィールドを描画する処理</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static void LabelValueField(string label, Action valueFieldFunc, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            if (valueFieldFunc == null) {
                return;
            }

            using (new GUILayout.HorizontalScope(guiLayoutOptions)) {
                var labelOptions = LabelFieldGuiLayoutOptions
                    .Concat(new[] { GUILayout.Width(LabelFieldWidth) })
                    .ToArray();
                GUILayout.Label(label, style, labelOptions);
                valueFieldFunc.Invoke();
            }
        }

        /// <summary>
        /// ラベルと値のフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="valueFieldFunc">右辺に表示する値フィールドを描画する処理</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static void LabelValueField(string label, Action valueFieldFunc, params GUILayoutOption[] guiLayoutOptions) {
            LabelValueField(label, valueFieldFunc, GUI.skin.label, guiLayoutOptions);
        }
        
        /// <summary>
        /// ラベルと値のフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="valueFieldFunc">右辺に表示する値フィールドを描画する処理</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static T LabelValueField<T>(string label, Func<T> valueFieldFunc, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            if (valueFieldFunc == null) {
                return default;
            }

            using (new GUILayout.HorizontalScope(guiLayoutOptions)) {
                var labelOptions = LabelFieldGuiLayoutOptions
                    .Concat(new[] { GUILayout.Width(LabelFieldWidth) })
                    .ToArray();
                GUILayout.Label(label, style, labelOptions);
                return valueFieldFunc.Invoke();
            }
        }

        /// <summary>
        /// ラベルと値のフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="valueFieldFunc">右辺に表示する値フィールドを描画する処理</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static T LabelValueField<T>(string label, Func<T> valueFieldFunc,
            params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, valueFieldFunc, GUI.skin.label, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(矢印順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="ignoreValues">除外するEnumの値</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumArrowOrderField<T>(string label, T enumValue, ICollection<T> ignoreValues, GUIStyle style, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return LabelValueField(label, () => {
                using (new GUILayout.HorizontalScope()) {
                    var diff = 0;

                    if (GUILayout.Button("<", GUILayout.ExpandWidth(false))) {
                        diff = -1;
                    }

                    GUILayout.Label(enumValue.ToString(), style, GUILayout.ExpandWidth(true));

                    if (GUILayout.Button(">", GUILayout.ExpandWidth(false))) {
                        diff = 1;
                    }

                    var enumValues = ((T[])Enum.GetValues(typeof(T)))
                        .Where(x => ignoreValues == null || !ignoreValues.Contains(x))
                        .ToList();
                    var index = enumValues.IndexOf(enumValue);
                    GUILayout.Label($"{index + 1}/{enumValues.Count}", GUI.skin.box,
                        GUILayout.ExpandWidth(false));

                    if (diff != 0) {
                        index = (index + diff + enumValues.Count) % enumValues.Count;
                        return enumValues[index];
                    }

                    return enumValue;
                }
            }, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(矢印順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="ignoreValues">除外するEnumの値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumArrowOrderField<T>(string label, T enumValue, ICollection<T> ignoreValues, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumArrowOrderField(label, enumValue, ignoreValues, GUI.skin.box, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(矢印順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumArrowOrderField<T>(string label, T enumValue, GUIStyle style, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumArrowOrderField(label, enumValue, null, style, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(矢印順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumArrowOrderField<T>(string label, T enumValue, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumArrowOrderField(label, enumValue, GUI.skin.box, guiLayoutOptions);
        }

        /// <summary>
        /// 選択式のフィールド描画(矢印順送り)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="index">現在選択されている物のIndex</param>
        /// <param name="valueLabels">選択候補のラベルリスト</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のIndex</returns>
        public static int ArrowOrderField(string label, int index, string[] valueLabels, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => {
                using (new GUILayout.HorizontalScope()) {
                    var diff = 0;

                    if (GUILayout.Button("<", GUILayout.ExpandWidth(false))) {
                        diff = -1;
                    }

                    GUILayout.Label(valueLabels[index], style, GUILayout.ExpandWidth(true));

                    if (GUILayout.Button(">", GUILayout.ExpandWidth(false))) {
                        diff = 1;
                    }

                    GUILayout.Label($"{index + 1}/{valueLabels.Length}", GUI.skin.box,
                        GUILayout.ExpandWidth(false));

                    if (diff != 0) {
                        index = (index + diff + valueLabels.Length) % valueLabels.Length;
                    }

                    return index;
                }
            }, guiLayoutOptions);
        }

        /// <summary>
        /// 選択式のフィールド描画(矢印順送り)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="index">現在選択されている物のIndex</param>
        /// <param name="valueLabels">選択候補のラベルリスト</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のIndex</returns>
        public static int ArrowOrderField(string label, int index, string[] valueLabels, params GUILayoutOption[] guiLayoutOptions) {
            return ArrowOrderField(label, index, valueLabels, GUI.skin.box, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(ボタン順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="ignoreValues">除外するEnumの値</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumButtonOrderField<T>(string label, T enumValue, ICollection<T> ignoreValues, GUIStyle style, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return LabelValueField(label, () => {
                using (new GUILayout.HorizontalScope()) {
                    var enumValues = ((T[])Enum.GetValues(typeof(T)))
                        .Where(x => ignoreValues == null || !ignoreValues.Contains(x))
                        .ToList();
                    var index = enumValues.IndexOf(enumValue);

                    if (GUILayout.Button(enumValue.ToString(), style)) {
                        index = (index + 1 + enumValues.Count) % enumValues.Count;
                        return enumValues[index];
                    }

                    GUILayout.Label($"{index + 1}/{enumValues.Count}", "Box", GUILayout.ExpandWidth(false));

                    return enumValue;
                }
            }, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(ボタン順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="ignoreValues">除外するEnumの値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumButtonOrderField<T>(string label, T enumValue, ICollection<T> ignoreValues, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumButtonOrderField(label, enumValue, ignoreValues, GUI.skin.button,
                guiLayoutOptions);
        }


        /// <summary>
        /// 列挙型のフィールド描画(ボタン順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumButtonOrderField<T>(string label, T enumValue, GUIStyle style, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumButtonOrderField(label, enumValue, null, style, guiLayoutOptions);
        }


        /// <summary>
        /// 列挙型のフィールド描画(ボタン順送り)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumButtonOrderField<T>(string label, T enumValue, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumButtonOrderField(label, enumValue, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// 選択式のフィールド描画(ボタン順送り)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="index">現在選択されている物のIndex</param>
        /// <param name="valueLabels">選択候補のラベルリスト</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のIndex</returns>
        public static int ButtonOrderField(string label, int index, string[] valueLabels, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => {
                using (new GUILayout.HorizontalScope()) {
                    if (GUILayout.Button(valueLabels[index], style)) {
                        index = (index + 1 + valueLabels.Length) % valueLabels.Length;
                    }

                    GUILayout.Label($"{index + 1}/{valueLabels.Length}", "Box", GUILayout.ExpandWidth(false));

                    return index;
                }
            }, guiLayoutOptions);
        }

        /// <summary>
        /// 選択式のフィールド描画(ボタン順送り)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="index">現在選択されている物のIndex</param>
        /// <param name="valueLabels">選択候補のラベルリスト</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のIndex</returns>
        public static int ButtonOrderField(string label, int index, string[] valueLabels, params GUILayoutOption[] guiLayoutOptions) {
            return ButtonOrderField(label, index, valueLabels, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(選択式)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="xCount">水平方向に並ぶ要素最大数</param>
        /// <param name="ignoreValues">除外するEnumの値</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumSelectionField<T>(string label, T enumValue, int xCount, ICollection<T> ignoreValues, GUIStyle style, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return LabelValueField(label, () => {
                var enumNames = Enum.GetNames(typeof(T));
                var enumValues = ((T[])Enum.GetValues(typeof(T)))
                    .Where(x => ignoreValues == null || !ignoreValues.Contains(x))
                    .ToList();
                var index = enumValues.IndexOf(enumValue);
                index = GUILayout.SelectionGrid(index, enumNames, xCount);
                return enumValues[index];
            }, style, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(選択式)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="xCount">水平方向に並ぶ要素最大数</param>
        /// <param name="ignoreValues">除外するEnumの値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumSelectionField<T>(string label, T enumValue, int xCount, ICollection<T> ignoreValues, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumSelectionField(label, enumValue, xCount, ignoreValues, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(選択式)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="xCount">水平方向に並ぶ要素最大数</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumSelectionField<T>(string label, T enumValue, int xCount, GUIStyle style, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumSelectionField(label, enumValue, xCount, null, style, guiLayoutOptions);
        }

        /// <summary>
        /// 列挙型のフィールド描画(選択式)
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="enumValue">現在のEnumの値</param>
        /// <param name="xCount">水平方向に並ぶ要素最大数</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のEnumの値</returns>
        public static T EnumSelectionField<T>(string label, T enumValue, int xCount, params GUILayoutOption[] guiLayoutOptions)
            where T : Enum {
            return EnumSelectionField(label, enumValue, xCount, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// 選択式のフィールド描画(選択式)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="index">現在選択されている物のIndex</param>
        /// <param name="valueLabels">選択候補のラベルリスト</param>
        /// <param name="xCount">横方向に並べる数</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のIndex</returns>
        public static int SelectionField(string label, int index, string[] valueLabels, int xCount, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => {
                index = GUILayout.SelectionGrid(index, valueLabels, xCount, style);
                return index;
            }, guiLayoutOptions);
        }

        /// <summary>
        /// 選択式のフィールド描画(選択式)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="index">現在選択されている物のIndex</param>
        /// <param name="valueLabels">選択候補のラベルリスト</param>
        /// <param name="xCount">横方向に並べる数</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後のIndex</returns>
        public static int SelectionField(string label, int index, string[] valueLabels, int xCount, params GUILayoutOption[] guiLayoutOptions) {
            return SelectionField(label, index, valueLabels, xCount, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// スライダーフィールド描画(Float)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <param name="displayFormat">表示フォーマット</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後の値</returns>
        public static float SliderField(string label, float value, float minValue, float maxValue, string displayFormat = "0.00", params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => {
                using (new GUILayout.HorizontalScope()) {
                    var result = GUILayout.HorizontalSlider(value, minValue, maxValue);
                    var style = GUI.skin.box;
                    style.CalcMinMaxWidth(new GUIContent(maxValue.ToString(displayFormat)), out _, out var maxWidth);
                    GUILayout.Label(result.ToString(displayFormat), style, GUILayout.Width(maxWidth));
                    return result;
                }
            }, guiLayoutOptions);
        }

        /// <summary>
        /// スライダーフィールド描画(Int)
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <param name="displayFormat">表示フォーマット</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後の値</returns>
        public static int SliderField(string label, int value, int minValue, int maxValue, string displayFormat = "0", params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => {
                using (new GUILayout.HorizontalScope()) {
                    var result = (int)GUILayout.HorizontalSlider(value, minValue, maxValue);
                    var style = GUI.skin.box;
                    style.CalcMinMaxWidth(new GUIContent(maxValue.ToString(displayFormat)), out _, out var maxWidth);
                    GUILayout.Label(result.ToString(displayFormat), style, GUILayout.Width(maxWidth));
                    return result;
                }
            }, guiLayoutOptions);
        }

        /// <summary>
        /// テキストフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="maxLength">最大入力文字数</param>"
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後の値</returns>
        public static string TextField(string label, string value, int maxLength, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => GUILayout.TextField(value, maxLength, style),
                guiLayoutOptions);
        }

        /// <summary>
        /// テキストフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="maxLength">最大入力文字数</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後の値</returns>
        public static string TextField(string label, string value, int maxLength, params GUILayoutOption[] guiLayoutOptions) {
            return TextField(label, value, maxLength, GUI.skin.textField, guiLayoutOptions);
        }

        /// <summary>
        /// テキストフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後の値</returns>
        public static string TextField(string label, string value, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => GUILayout.TextField(value, style), guiLayoutOptions);
        }

        /// <summary>
        /// テキストフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        /// <returns>変更後の値</returns>
        public static string TextField(string label, string value, params GUILayoutOption[] guiLayoutOptions) {
            return TextField(label, value, GUI.skin.textField, guiLayoutOptions);
        }

        /// <summary>
        /// ラベルフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="style">Style</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static void LabelField(string label, string value, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            LabelValueField(label, () => GUILayout.Label(value, style), guiLayoutOptions);
        }

        /// <summary>
        /// ラベルフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static void LabelField(string label, string value, params GUILayoutOption[] guiLayoutOptions) {
            LabelField(label, value, GUI.skin.label, guiLayoutOptions);
        }

        /// <summary>
        /// ボタン描画（色指定）
        /// </summary>
        /// <param name="buttonLabel">ボタンに表示するラベル</param>
        /// <param name="buttonColor">ボタンのGUIColor</param>
        /// <param name="style">GUIのスタイル</param>
        /// <param name="guiLayoutOptions">レイアウト拡張用オプション</param>
        public static bool Button(string buttonLabel, Color buttonColor, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            var prevColor = GUI.color;
            GUI.color = buttonColor;
            var result = GUILayout.Button(buttonLabel, style, guiLayoutOptions);
            GUI.color = prevColor;
            return result;
        }

        /// <summary>
        /// ボタン描画（色指定）
        /// </summary>
        /// <param name="buttonLabel">ボタンに表示するラベル</param>
        /// <param name="buttonColor">ボタンのGUIColor</param>
        /// <param name="guiLayoutOptions">レイアウト拡張用オプション</param>
        public static bool Button(string buttonLabel, Color buttonColor, params GUILayoutOption[] guiLayoutOptions) {
            return Button(buttonLabel, buttonColor, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// ボタンフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="buttonLabel">ボタンに表示するラベル</param>
        /// <param name="buttonColor">ボタンのGUIColor</param>
        /// <param name="style">GUIのスタイル</param>
        /// <param name="guiLayoutOptions">レイアウト拡張用オプション</param>
        public static bool ButtonField(string label, string buttonLabel, Color buttonColor, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => {
                var prevColor = GUI.color;
                GUI.color = buttonColor;
                var result = GUILayout.Button(buttonLabel, style);
                GUI.color = prevColor;
                return result;
            }, guiLayoutOptions);
        }

        /// <summary>
        /// ボタンフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="buttonLabel">ボタンに表示するラベル</param>
        /// <param name="buttonColor">ボタンのGUIColor</param>
        /// <param name="guiLayoutOptions">レイアウト拡張用オプション</param>
        public static bool ButtonField(string label, string buttonLabel, Color buttonColor, params GUILayoutOption[] guiLayoutOptions) {
            return ButtonField(label, buttonLabel, buttonColor, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// ボタンフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="buttonLabel">ボタンに表示するラベル</param>
        /// <param name="style">GUIのスタイル</param>
        /// <param name="guiLayoutOptions">レイアウト拡張用オプション</param>
        public static bool ButtonField(string label, string buttonLabel, GUIStyle style, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => GUILayout.Button(buttonLabel, style), guiLayoutOptions);
        }

        /// <summary>
        /// ボタンフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="buttonLabel">ボタンに表示するラベル</param>
        /// <param name="guiLayoutOptions">レイアウト拡張用オプション</param>
        public static bool ButtonField(string label, string buttonLabel, params GUILayoutOption[] guiLayoutOptions) {
            return ButtonField(label, buttonLabel, GUI.skin.button, guiLayoutOptions);
        }

        /// <summary>
        /// トグルボタンフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="flagValue">フラグ値</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static bool ToggleButtonField(string label, bool flagValue, params GUILayoutOption[] guiLayoutOptions) {
            return ToggleButtonField(label, flagValue, "On", "Off", guiLayoutOptions);
        }

        /// <summary>
        /// トグルボタンフィールド描画
        /// </summary>
        /// <param name="label">左辺に表示されるラベル</param>
        /// <param name="flagValue">フラグ値</param>
        /// <param name="onLabel">ONの時のボタンのラベル</param>
        /// <param name="offLabel">OFFの時のボタンのラベル</param>
        /// <param name="guiLayoutOptions">フィールド全体のLayoutOption</param>
        public static bool ToggleButtonField(string label, bool flagValue, string onLabel, string offLabel, params GUILayoutOption[] guiLayoutOptions) {
            return LabelValueField(label, () => { return ToggleButton(flagValue, onLabel, offLabel); }, guiLayoutOptions);
        }

        /// <summary>
        /// トグルボタンフィールド描画
        /// </summary>
        /// <param name="flagValue">フラグ値</param>
        /// <param name="onLabel">ONの時のボタンのラベル</param>
        /// <param name="offLabel">OFFの時のボタンのラベル</param>
        public static bool ToggleButton(bool flagValue, string onLabel, string offLabel) {
            var buttonTitle = flagValue ? onLabel : offLabel;
            var prevColor = GUI.color;
            GUI.color = flagValue ? Color.green : Color.gray;

            if (GUILayout.Button(buttonTitle)) {
                flagValue ^= true;
            }

            GUI.color = prevColor;
            return flagValue;
        }

        /// <summary>
        /// 反映ボタン付きのスライダー描画
        /// </summary>
        /// <param name="label">最上段に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <param name="onApply">反映時の処理</param>
        /// <param name="displayFormat">表示フォーマット</param>
        /// <returns>変更値</returns>
        public static float SliderWithApplyButton(string label, float value, float minValue, float maxValue, Action<float> onApply, string displayFormat = "0.00") {
            float result;
            GUILayout.Label(label);

            using (new GUILayout.HorizontalScope()) {
                result = GUILayout.HorizontalSlider(value, minValue, maxValue);
                var style = new GUIStyle("Box");
                style.CalcMinMaxWidth(new GUIContent(maxValue.ToString(displayFormat)), out _, out var maxWidth);
                GUILayout.Label(result.ToString(displayFormat), style, GUILayout.Width(maxWidth));

                if (GUILayout.Button("Apply", GUILayout.ExpandWidth(false))) {
                    onApply?.Invoke(result);
                }
            }

            return result;
        }

        /// <summary>
        /// 反映ボタン付きのスライダー描画（Int版）
        /// </summary>
        /// <param name="label">最上段に表示されるラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <param name="onApply">反映時の処理</param>
        /// <param name="displayFormat">表示フォーマット</param>
        /// <returns>変更値</returns>
        public static int SliderWithApplyButton(string label, int value, int minValue, int maxValue, Action<int> onApply, string displayFormat = "0") {
            int result;
            GUILayout.Label(label);

            using (new GUILayout.HorizontalScope()) {
                result = (int)GUILayout.HorizontalSlider(value, minValue, maxValue);
                var style = new GUIStyle("Box");
                style.CalcMinMaxWidth(new GUIContent(maxValue.ToString(displayFormat)), out _, out var maxWidth);
                GUILayout.Label(result.ToString(displayFormat), style, GUILayout.Width(maxWidth));

                if (GUILayout.Button("Apply", GUILayout.ExpandWidth(false))) {
                    onApply?.Invoke(result);
                }
            }

            return result;
        }

        /// <summary>
        /// フィルタの描画
        /// </summary>
        public static string DrawFilter(string filterKey, string label = "Filter") {
            if (!Filters.TryGetValue(filterKey, out var filter)) {
                Filters[filterKey] = "";
            }

            GUILayout.Label(label);
            filter = GUILayout.TextField(filter);
            GUILayout.Space(5);

            Filters[filterKey] = filter;
            return filter;
        }
    }
}