using System.Text;

namespace SevenDev.Utility;

public static class StringExtensions {
	public static string ToSnakeCase(this string str) {
		if (string.IsNullOrEmpty(str)) return str;

		StringBuilder stringBuilder = new();
		bool wasPreviousUpper = false;

		for (int i = 0; i < str.Length; i++) {
			char currentChar = str[i];

			if (char.IsUpper(currentChar)) {
				if (i > 0 && (!wasPreviousUpper || i < str.Length - 1 && char.IsLower(str[i + 1]))) {
					stringBuilder.Append('_');
				}
				stringBuilder.Append(char.ToLower(currentChar));
				wasPreviousUpper = true;
			}
			else if (char.IsWhiteSpace(currentChar) || currentChar == '-' || currentChar == '.' || currentChar == '_') {
				if (stringBuilder.Length > 0 && stringBuilder[^1] != '_') {
					stringBuilder.Append('_');
				}
				wasPreviousUpper = false;
			}
			else {
				stringBuilder.Append(currentChar);
				wasPreviousUpper = false;
			}
		}

		// Ensure the final string doesn't end with an underscore
		if (stringBuilder.Length > 0 && stringBuilder[^1] == '_') {
			stringBuilder.Length--;
		}

		return stringBuilder.ToString();
	}
}