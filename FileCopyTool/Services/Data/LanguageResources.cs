namespace FileCopyTool.Services.Data
{
	public static class LanguageResources
	{
		public static readonly string[] SupportedLanguages = ["EN", "JP"];
		public static readonly string[] SupportedLanguageTitles = ["English (EN)", "日本語 (JP)"];
		public static readonly string DefaultLanguage = "EN";

		private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
		{
			{
				"EN", new Dictionary<string, string>
				{
					{ "LanguageTitle", "English (EN)" },
					{ "FormTitle", "File Copy Tool (Ctrl + Shift + T to perform copy)" },
					{ "LabelFrom", "From\n(Multiple files accepted)" },
					{ "LabelTo", "To\n(Single folder path)" },
					{ "LabelNote", "※ Run File Explorer as Administrator for drag-and-drop." },
					{ "ButtonExit", "Exit" },
					{ "ButtonMinimize", "Minimize" },
					{ "ButtonCopy", "Perform Copy" },
					{ "ButtonAddRow", "Add Row" },
					{ "ButtonBrowse", "Browse" },
					{ "ButtonDelete", "Delete" },
					{ "MessageCopySuccess", "Files copied successfully!" },
					{ "MessageCopyWarning", "Copy operation completed, but some errors occurred!" },
					{ "MessageCopyError", "Error copying files: {0}" },
					{ "MessageConfigError", "Error {0} configurations: {1}" },
					{ "MessageInstanceRunning", "Another instance of FileCopyTool is already running." },
					{ "MessageSuccess", "Success" },
					{ "MessageWarning", "Warning" },
					{ "MessageError", "Error" },
					{ "ToolsMenu", "Tools" },
					{ "LanguageMenu", "Language" },
					{ "SettingsMenu", "Settings" },
					{ "PopupMenu", "Popup"},
					{ "PopupSettingAll", "All"},
					{ "PopupSettingWarningOrError", "Only when warning or error" },
					{ "PopupSettingErrorOnly", "Only when error" },
					{ "PopupSettingNever", "Never" }
				}
			},
			{
				"JP", new Dictionary<string, string>
				{
					{ "LanguageTitle", "日本語 (JP)" },
					{ "FormTitle", "ファイルコピーツール (Ctrl + Shift + T でコピー実行)" },
					{ "LabelFrom", "コピー元\n(複数ファイル可)" },
					{ "LabelTo", "コピー先\n（単一フォルダパス）" },
					{ "LabelNote", "※ ドラッグ＆ドロップには管理者権限でエクスプローラーを実行してください。" },
					{ "ButtonExit", "終了" },
					{ "ButtonMinimize", "最小化" },
					{ "ButtonCopy", "コピー実行" },
					{ "ButtonAddRow", "行を追加" },
					{ "ButtonBrowse", "参照" },
					{ "ButtonDelete", "削除" },
					{ "MessageCopySuccess", "ファイルのコピーが成功しました！" },
					{ "MessageCopyWarning", "コピー操作が完了しましたが、いくつかのエラーが発生しました！" },
					{ "MessageCopyError", "ファイルのコピーでエラー：{0}" },
					{ "MessageConfigError", "設定の{0}でエラー：{1}" },
					{ "MessageInstanceRunning", "FileCopyToolの別のインスタンスが既に実行中です。" },
					{ "MessageSuccess", "成功" },
					{ "MessageWarning", "警告" },
					{ "MessageError", "エラー" },
					{ "ToolsMenu", "ツール" },
					{ "LanguageMenu", "言語" },
					{ "SettingsMenu", "設定" },
					{ "PopupMenu", "ポップアップ"},
					{ "PopupSettingAll", "全て"},
					{ "PopupSettingWarningOrError", "報告とエラーのみ" },
					{ "PopupSettingErrorOnly", "エラーのみ" },
					{ "PopupSettingNever", "なし" }
				}
			}
		};

		public static string GetString(string key, string language)
		{
			return Translations.TryGetValue(language, out var dict) && dict.TryGetValue(key, out var value)
				? value
				: Translations[DefaultLanguage][key];
		}
	}
}