using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Exceptions;

namespace Resto.Front.Api.SamplePlugin;

/// <summary>
/// Example implementation of <see cref="IIdentifierCodeHandler"/> that accepts GTIN codes consisting of 13 or 14 digits.
///
/// Supported formats:
/// - GTIN-13 (e.g., EAN-13): 13 digits
/// - GTIN-14: 14 digits
///
/// The sample verification method returns a successful result if the code does not end with '0'.
///
/// Regex: ^\d{13,14}$
///
/// Examples:
/// - 4601234567891   → supported, IsAllowed = true
/// - 01234567890123  → supported, IsAllowed = true
/// - 00000046198488!/Jsn:xABm80j4k  → supported, IsAllowed = true, hard coded, Tobaco ENFORCE_BELARUS_FISCALIZATION = false;
/// - AAAA123456      → supported, IsAllowed = true, hard coded, ENFORCE_UKRAINE_FISCALIZATION = true
/// - 4601234567890   → supported, IsAllowed = false (ends with '0')
/// - 123456789012    → not supported (only 12 digits)
/// - ABC1234567890   → not supported (contains non-digit characters)
/// </summary>
internal sealed class MarkingCodesHandler : IIdentifierCodeHandler
{
    public string SourceKey { get; } = "gtin";
    public string Description { get; } = "Handler for GTIN codes";

    private static readonly Regex GtinRegex = new(@"^\d{13,14}$", RegexOptions.Compiled);

    public IdentifierCodeSupportResult CheckIdentifierCodeSupport(
        IOrder order,
        IOrderProductItem orderItem,
        string code, bool isDeleteMark)
    {
        if (CheckGtin(code))
        {
            // All matching GTINs must be verified and sent to fiscal device
            return new IdentifierCodeSupportResult()
            {
                IsSupported = true,
                CodeHandlerFlags = IdentifierCodeHandlerFlags.RequiresVerification |
                                   IdentifierCodeHandlerFlags.RequiresSendToFiscal,
                Message = $"Code accepted! Is delete mark: {isDeleteMark}"
            };
        }

        return new IdentifierCodeSupportResult()
            { IsSupported = false, CodeHandlerFlags = IdentifierCodeHandlerFlags.None, Message = $"Code rejected! Is delete mark: {isDeleteMark}" };
    }

    public IdentifierCodeSupportResult CheckIdentifierCodeSupport(
        IOrder order,
        IOrderModifierItem modifierItem,
        IOrderProductItem parentOrderItem,
        string code, bool isDeleteMark)
    {
        return CheckIdentifierCodeSupport(order, parentOrderItem, code, isDeleteMark);
    }

    public CodeVerificationResult VerifyIdentifierCode(
        IOrder order,
        IOrderProductItem orderItem,
        OrderItemIdentifierCode code)
    {
        return VerifyGtin(code.Code.Value);
    }

    public CodeVerificationResult VerifyIdentifierCode(
        IOrder order,
        IOrderModifierItem modifierItem,
        IOrderProductItem parentOrderItem,
        OrderItemIdentifierCode code)
    {
        return VerifyGtin(code.Code.Value);
    }

    /// <summary>
    /// Mock verification: all valid GTINs are allowed unless they end with '0'.
    /// </summary>
    private CodeVerificationResult VerifyGtin(string code)
    {
        var result = new CodeVerificationResult
        {
            Parameters = new Dictionary<string, string>
            {
                { "Handler", SourceKey },
                { "CodeLength", code.Length.ToString() }
            }
        };
        
        // Check if last character is '0' -> not allowed
        if (code.EndsWith("0"))
        {
            result.IsAllowed = false;
            result.UserMessage = "Rejected: GTIN ends with 0";
        }
        else
        {
            result.IsAllowed = true;
            result.UserMessage = "Accepted";
        }

        return result;
    }

    private bool CheckGtin (string code)
    {
        //Tobaco if Tobaco ENFORCE_BELARUS_FISCALIZATION = false
        if (code == "00000046198488!/Jsn:xABm80j4k")
            return true;
        //ENFORCE_UKRAINE_FISCALIZATION = true
        if (code == "AAAA123456")
            return true;

        return GtinRegex.IsMatch(code);
    }

    public static IDisposable Register()
    {
        IDisposable subscription = null;
        try
        {
            subscription = PluginContext.Operations.RegisterIdentifierCodeHandler(new MarkingCodesHandler());
            PluginContext.Log.Info("MarkingCodesHandler was registered.");
        }
        catch (LicenseRestrictionException ex)
        {
            PluginContext.Log.Warn(ex.Message);
        }

        return subscription;
    }

}
