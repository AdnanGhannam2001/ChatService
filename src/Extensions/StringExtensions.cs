using System.Text;

namespace ChatService.Extensions;

internal static class StringExtensions {
    public static string ToKebabCase(this string value) {
        var kebabCase = new StringBuilder();

        for (var i = 0; i < value.Length; i++) {
            if (char.IsUpper(value[i])) {
                if (i != 0) kebabCase.Append('-');

                kebabCase.Append(char.ToLower(value[i]));
            }
            else {
                kebabCase.Append(value[i]);
            }
        }

        return kebabCase.ToString();
    }
}