namespace Resto.Front.Api.CustomerScreen.Helpers
{
    internal static class AmountHelper
    {
        public static string CalculateModifierAmountString(decimal modifierAmount, int defaultAmount, bool hideIfDefaultAmount, bool isPaid, bool isAmountIndependentOfParentAmount)
        {
            // Настройка способа отображения количества групповых модификаторов блюда.	
            var showDeltaAmount = PluginContext.Operations.GetHostRestaurant().DisplayRelativeNumberOfModifiers;

            // Если включена опция "Количество не зависит от количества блюда", то всегда пишем "+N".
            if (isAmountIndependentOfParentAmount)
                return $"+{modifierAmount}";

            // Если модификатор платный или показываем абсолютное количество модификаторов, то пишем "×N".
            const string charX = "\u00D7";
            var multiplyAmountString = $"{charX}{modifierAmount}";

            if (isPaid || !showDeltaAmount)
                return multiplyAmountString;

            // Показываем относительное количество модификаторов.
            var deltaAmount = modifierAmount - defaultAmount;

            switch (deltaAmount)
            {
                case 1:
                    return "+";
                case -1:
                    return "-";
                case 0 when hideIfDefaultAmount:
                    return string.Empty;
                case 0:
                    return multiplyAmountString;
                default:
                    return $"{deltaAmount:+#;-#;0}";
            }
        }
    }
}
