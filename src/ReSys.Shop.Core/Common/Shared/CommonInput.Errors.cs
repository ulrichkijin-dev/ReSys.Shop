using ReSys.Shop.Core.Common.Extensions;

namespace ReSys.Shop.Core.Common.Shared;

public static partial class CommonInput
{
    #region Errors

    public static class Errors
    {
        #region Generic

        public static Error Required(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(Required)}",
                description: msg ?? string.Format(format: ValidationMessages.General.Required, arg0: Label(prefix: prefix, field: field)));

        public static Error NotFound(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(NotFound)}",
                description: msg ?? string.Format(format: ValidationMessages.General.NotFound, arg0: Label(prefix: prefix, field: field)));

        public static Error AlreadyExists(string? prefix = null, string? field = null, string? identifier = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(AlreadyExists)}",
                description: msg ?? string.Format(format: ValidationMessages.General.AlreadyExists, arg0: Label(prefix: prefix, field: field), arg1: identifier));

        public static Error Conflict(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(Conflict)}",
                description: msg ?? string.Format(format: ValidationMessages.General.Conflict, arg0: Label(prefix: prefix, field: field), arg1: string.Empty));

        public static Error InvalidOperation(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidOperation)}",
                description: msg ?? string.Format(format: ValidationMessages.General.InvalidOperation, arg0: Label(prefix: prefix, field: field), arg1: string.Empty));

        public static Error NotAuthorized(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(NotAuthorized)}",
                description: msg ?? string.Format(format: ValidationMessages.General.NotAuthorized, arg0: Label(prefix: prefix, field: field)));

        public static Error Forbidden(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(Forbidden)}",
                description: msg ?? string.Format(format: ValidationMessages.General.Forbidden, arg0: Label(prefix: prefix, field: field)));

        public static Error RelationshipConstraintViolation(string? prefix = null, string? field = null, string? action = null, string? relatedEntity = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(RelationshipConstraintViolation)}",
                description: msg ?? string.Format(format: ValidationMessages.General.RelationshipConstraintViolation, arg0: action, arg1: relatedEntity));

        public static Error InsufficientPermissions(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InsufficientPermissions)}",
                description: msg ?? string.Format(format: ValidationMessages.General.InsufficientPermissions, arg0: Label(prefix: prefix, field: field)));

        public static Error ServiceUnavailable(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(ServiceUnavailable)}",
                description: msg ?? ValidationMessages.General.ServiceUnavailable);

        public static Error RateLimitExceeded(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(RateLimitExceeded)}",
                description: msg ?? ValidationMessages.General.RateLimitExceeded);

        public static Error FeatureDisabled(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(FeatureDisabled)}",
                description: msg ?? string.Format(format: ValidationMessages.General.FeatureDisabled, arg0: Label(prefix: prefix, field: field)));

        public static Error Null(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(Null)}",
                description: msg ?? string.Format(format: ValidationMessages.General.Null, arg0: Label(prefix: prefix, field: field)));

        public static Error NullOrEmpty(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(NullOrEmpty)}",
                description: msg ?? string.Format(format: ValidationMessages.General.NullOrEmpty, arg0: Label(prefix: prefix, field: field)));

        public static Error TooShort(string? prefix = null, string? field = null, int minLength = Constraints.Text.MinLength,
            string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(TooShort)}",
                description: msg ??
                             string.Format(format: ValidationMessages.General.TooShort, arg0: Label(prefix: prefix, field: field), arg1: minLength));

        public static Error TooLong(string? prefix = null, string? field = null, int maxLength = Constraints.Text.MaxLength,
            string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(TooLong)}",
                description: msg ??
                             string.Format(format: ValidationMessages.General.TooLong, arg0: Label(prefix: prefix, field: field), arg1: maxLength));

        public static Error InvalidRange(string? prefix = null, string? field = null, object? min = null,
            object? max = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidRange)}",
                description: msg ?? string.Format(format: ValidationMessages.General.InvalidRange, arg0: Label(prefix: prefix, field: field), arg1: min, arg2: max));

        public static Error InvalidPattern(string? prefix = null, string? field = null,
            string? formatDescription = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidPattern)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Text.InvalidPattern, arg0: Label(prefix: prefix, field: field), arg1: formatDescription != null ? $" ({formatDescription})" : ""));

        public static Error InvalidAllowedPattern(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidAllowedPattern)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Text.InvalidAllowedPattern, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidValue(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidValue)}",
                description: msg ?? string.Format(format: ValidationMessages.General.InvalidValue, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Contact

        public static Error InvalidEmail(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidEmail)}",
                description: msg ?? string.Format(format: ValidationMessages.Contact.InvalidEmail, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidPhone(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidPhone)}",
                description: msg ?? string.Format(format: ValidationMessages.Contact.InvalidPhone, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidUrl(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidUrl)}",
                description: msg ?? string.Format(format: ValidationMessages.Contact.InvalidUrl, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidUri(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidUri)}",
                description: msg ?? string.Format(format: ValidationMessages.Contact.InvalidUri, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region User

        public static Error InvalidName(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidName)}",
                description: msg ?? string.Format(format: ValidationMessages.User.InvalidName, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidUsername(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidUsername)}",
                description: msg ??
                             string.Format(format: ValidationMessages.User.InvalidUsername, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Identifier

        public static Error InvalidGuid(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidGuid)}",
                description: msg ?? string.Format(format: ValidationMessages.Identifier.InvalidGuid, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidUlid(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidUlid)}",
                description: msg ?? string.Format(format: ValidationMessages.Identifier.InvalidUlid, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidNanoId(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidNanoId)}",
                description: msg ?? string.Format(format: ValidationMessages.Identifier.InvalidNanoId, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Network

        public static Error InvalidIpAddress(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidIpAddress)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Network.InvalidIpAddress, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidMacAddress(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidMacAddress)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Network.InvalidMacAddress, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidDomain(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidDomain)}",
                description: msg ?? string.Format(format: ValidationMessages.Network.InvalidDomain, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Geographic

        public static Error InvalidPostalCode(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidPostalCode)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Geographic.InvalidPostalCode, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidZipCode(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidZipCode)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Geographic.InvalidZipCode, arg0: Label(prefix: prefix, field: field)));

        public static Error
            InvalidCanadianPostalCode(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidCanadianPostalCode)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Geographic.InvalidCanadianPostalCode, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidUkPostalCode(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidUkPostalCode)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Geographic.InvalidUkPostalCode, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Security

        public static Error InvalidPassword(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidPassword)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Security.InvalidPassword, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Slugs and Versions

        public static Error InvalidSlug(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidSlug)}",
                description: msg ?? string.Format(format: ValidationMessages.SlugsAndVersions.InvalidSlug, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidSemVer(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidSemVer)}",
                description: msg ??
                             string.Format(format: ValidationMessages.SlugsAndVersions.InvalidSemVer, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidVersion(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidVersion)}",
                description: msg ??
                             string.Format(format: ValidationMessages.SlugsAndVersions.InvalidVersion, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Social

        public static Error InvalidTwitterHandle(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidTwitterHandle)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Social.InvalidTwitterHandle, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidHashtag(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidHashtag)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Social.InvalidHashtag, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Date and Time

        public static Error InvalidDate(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidDate)}",
                description: msg ?? string.Format(format: ValidationMessages.DateAndTime.InvalidDate, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidTime(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidTime)}",
                description: msg ?? string.Format(format: ValidationMessages.DateAndTime.InvalidTime, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidTimeSpan(string? prefix = null, string? field = null, string? msg = null) =>

            Error.Validation(

                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidTimeSpan)}",

                description: msg ??

                             string.Format(format: ValidationMessages.DateAndTime.InvalidTimeSpan, arg0: Label(prefix: prefix, field: field)));

        public static Error DateOffsetOutOfRange(
            string? prefix = null,
            string? field = null,
            DateTimeOffset? min = null,
            DateTimeOffset? max = null,
            string? customMessage = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(DateOffsetOutOfRange)}",
                description: customMessage ??
                             string.Format(format: ValidationMessages.DateAndTime.DateOffsetOutOfRange, arg0: Label(prefix: prefix, field: field), arg1: min.FormatUtc(), arg2: max.FormatUtc()));

        public static Error DateOffsetOutOfExclusiveRange(
            string? prefix = null,
            string? field = null,
            DateTimeOffset? min = null,
            DateTimeOffset? max = null,
            string? customMessage = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(DateOffsetOutOfExclusiveRange)}",
                description: customMessage ??
                             string.Format(format: ValidationMessages.DateAndTime.DateOffsetOutOfExclusiveRange, arg0: Label(prefix: prefix, field: field), arg1: min.FormatUtc(), arg2: max.FormatUtc()));

        public static Error MustBeInFuture(string? prefix = null, string? field = null, string? customMessage = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(MustBeInFuture)}",
                description: customMessage ??
                             string.Format(format: ValidationMessages.DateAndTime.MustBeInFuture, arg0: Label(prefix: prefix, field: field)));

        public static Error MustBeInPast(string? prefix = null, string? field = null, string? customMessage = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(MustBeInPast)}",
                description: customMessage ??
                             string.Format(format: ValidationMessages.DateAndTime.MustBeInPast, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Data

        public static Error InvalidJson(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidJson)}",
                description: msg ?? string.Format(format: ValidationMessages.Data.InvalidJson, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidBoolean(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidBoolean)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Data.InvalidBoolean, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Coordinates

        public static Error InvalidLatitude(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidLatitude)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Geographic.InvalidLatitude, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidLongitude(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidLongitude)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Geographic.InvalidLongitude, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Financial

        public static Error InvalidCreditCard(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidCreditCard)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Financial.InvalidCreditCard, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidCvv(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidCvv)}",
                description: msg ?? string.Format(format: ValidationMessages.Financial.InvalidCvv, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Visual

        public static Error InvalidHexColor(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidHexColor)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Visual.InvalidHexColor, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region File

        public static Error InvalidFilePath(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidFilePath)}",
                description: msg ??
                             string.Format(format: ValidationMessages.File.InvalidFilePath, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidFileExtension(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidFileExtension)}",
                description: msg ??
                             string.Format(format: ValidationMessages.File.InvalidFileExtension, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Localization

        public static Error InvalidCurrencyCode(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidCurrencyCode)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Localization.InvalidCurrencyCode, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidLanguageCode(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidLanguageCode)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Localization.InvalidLanguageCode, arg0: Label(prefix: prefix, field: field)));

        #endregion

        #region Numeric

        public static Error InvalidInteger(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidInteger)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Numeric.InvalidInteger, arg0: Label(prefix: prefix, field: field)));

        public static Error InvalidDecimal(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(code: $"{Prefix(prefix: prefix, field: field)}.{nameof(InvalidDecimal)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Numeric.InvalidDecimal, arg0: Label(prefix: prefix, field: field)));

        public static Error OutOfRange(string? prefix = null, string? field = null, object? minValue = null,
            object? maxValue = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(OutOfRange)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Numeric.OutOfRange, arg0: Label(prefix: prefix, field: field), arg1: minValue, arg2: maxValue));

        #endregion

        #region Text Content

        public static Error TitleTooLong(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(TitleTooLong)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Text.TitleTooLong, arg0: Label(prefix: prefix, field: field), arg1: Constraints.Text.TitleMaxLength));

        public static Error DescriptionTooLong(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(DescriptionTooLong)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Text.DescriptionTooLong, arg0: Label(prefix: prefix, field: field), arg1: Constraints.Text.DescriptionMaxLength));

        public static Error CommentTooLong(string? prefix = null, string? field = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(CommentTooLong)}",
                description: msg ??
                             string.Format(format: ValidationMessages.Text.CommentTooLong, arg0: Label(prefix: prefix, field: field), arg1: Constraints.Text.CommentMaxLength));

        #endregion
        #region Dictionary

        public static Error TooManyEntries(string? prefix = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix)}.TooManyEntries",
                description: msg ??
                             string.Format(format: ValidationMessages.Dictionary.TooManyEntries, arg0: Label(prefix: prefix), arg1: Constraints.Dictionary.MaxEntries));

        public static Error KeyRequired(string? prefix = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix)}.KeyRequired",
                description: msg ?? string.Format(format: ValidationMessages.Dictionary.KeyRequired, arg0: Label(prefix: prefix)));

        public static Error KeyInvalidPattern(string? prefix = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix)}.KeyInvalidPattern",
                description: msg ??
                             string.Format(format: ValidationMessages.Dictionary.KeyInvalidPattern, arg0: Label(prefix: prefix), arg1: Constraints.Dictionary.KeyAllowedPattern));

        public static Error KeyInvalidLength(string? prefix = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix)}.KeyInvalidLength",
                description: msg ??
                             string.Format(format: ValidationMessages.Dictionary.KeyInvalidLength, arg0: Label(prefix: prefix), arg1: Constraints.Dictionary.KeyMinLength, arg2: Constraints.Dictionary.KeyMaxLength));


        public static Error ValueInvalidLength(string? prefix = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix)}.ValueInvalidLength",
                description: msg ??
                             string.Format(format: ValidationMessages.Dictionary.ValueInvalidLength, arg0: Label(prefix: prefix), arg1: Constraints.Dictionary.ValueMinLength, arg2: Constraints.Dictionary.ValueMaxLength));


        #endregion

        #region Enum

        public static Error InvalidEnumValue<TEnum>(
            string? prefix = null,
            string? field = null,
            string? message = null)
            where TEnum : struct, Enum
        {
            string enumName = typeof(TEnum).Name;
            string validValues = EnumDescriptionExtensions.GetEnumContextDescription<TEnum>();
            return Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field ?? enumName)}.InvalidEnumValue",
                description: message ?? string.Format(format: ValidationMessages.Enum.InvalidEnumValue, arg0: enumName, arg1: validValues));
        }

        /// <summary>
        /// Creates a typed invalid flag combination error, automatically including valid bitmask descriptions.
        /// </summary>
        public static Error InvalidFlagCombination<TEnum>(
            string? prefix = null,
            string? field = null,
            string? message = null)
            where TEnum : struct, Enum
        {
            string enumName = typeof(TEnum).Name;
            string validFlags = EnumDescriptionExtensions.GetEnumContextDescription<TEnum>();
            return Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field ?? enumName)}.InvalidFlagCombination",
                description: message ?? string.Format(format: ValidationMessages.Enum.InvalidFlagCombination, arg0: enumName, arg1: validFlags));
        }

        #region Collections

        public static Error EmptyCollection(string? prefix = null, string? field = null, string? customMessage = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(EmptyCollection)}",
                description: customMessage ?? string.Format(format: ValidationMessages.Collections.EmptyCollection, arg0: Label(prefix: prefix, field: field)));

        public static Error TooFewItems(string? prefix = null, string? field = null, object? min = null, string? msg = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(TooFewItems)}",
                description: msg ?? string.Format(format: ValidationMessages.Collections.TooFewItems, arg0: Label(prefix: prefix, field: field), arg1: min));


        public static Error TooManyItems(string? prefix = null, string? field = null, long max = long.MaxValue, string? customMessage = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(TooManyItems)}",
                description: customMessage ?? string.Format(format: ValidationMessages.Collections.TooManyItems, arg0: Label(prefix: prefix, field: field), arg1: max));


        public static Error DuplicateItems(string? prefix = null, string? field = null, string? customMessage = null) =>
            Error.Validation(
                code: $"{Prefix(prefix: prefix, field: field)}.{nameof(DuplicateItems)}",
                description: customMessage ?? string.Format(format: ValidationMessages.Collections.DuplicateItems, arg0: Label(prefix: prefix, field: field)));

        #endregion
        #endregion

    }
    #endregion

}