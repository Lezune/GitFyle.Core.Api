﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using GitFyle.Core.Api.Models.Foundations.Sources;
using GitFyle.Core.Api.Models.Foundations.Sources.Exceptions;

namespace GitFyle.Core.Api.Services.Foundations.Sources
{
    internal partial class SourceService
    {
        private async ValueTask ValidateSourceOnAddAsync(Source source)
        {
            ValidateSourceIsNotNull(source);

            Validate(
                (Rule: await IsInvalidAsync(source.Id), Parameter: nameof(Source.Id)),
                (Rule: await IsInvalidAsync(source.Name), Parameter: nameof(Source.Name)),
                (Rule: await IsInvalidAsync(source.CreatedBy), Parameter: nameof(Source.CreatedBy)),
                (Rule: await IsInvalidAsync(source.UpdatedBy), Parameter: nameof(Source.UpdatedBy)),
                (Rule: await IsInvalidAsync(source.CreatedDate), Parameter: nameof(Source.CreatedDate)),
                (Rule: await IsInvalidAsync(source.UpdatedDate), Parameter: nameof(Source.UpdatedDate)),
                (Rule: await IsInvalidLengthAsync(source.Name, 255), Parameter: nameof(Source.Name)),
                (Rule: await IsInvalidUrlAsync(source.Url), Parameter: nameof(Source.Url)),

                (Rule: await IsValuesNotSameAsync(
                    createBy: source.UpdatedBy,
                    updatedBy: source.CreatedBy,
                    createdByName: nameof(Source.CreatedBy)),

                Parameter: nameof(Source.UpdatedBy)),

                (Rule: await IsDatesNotSameAsync(
                    createdDate: source.CreatedDate,
                    updatedDate: source.UpdatedDate,
                    nameof(Source.CreatedDate)),

                Parameter: nameof(Source.UpdatedDate)),

                (Rule: await IsNotRecentAsync(source.CreatedDate), Parameter: nameof(Source.CreatedDate)));
        }

        private async ValueTask<dynamic> IsNotRecentAsync(DateTimeOffset date)
        {
            var (isNotRecent, startDate, endDate) = await IsDateNotRecentAsync(date);

            return new
            {
                Condition = isNotRecent,
                Message = $"Date is not recent. Expected a value between {startDate} and {endDate} but found {date}"
            };
        }

        private async ValueTask<(bool IsNotRecent, DateTimeOffset StartDate, DateTimeOffset EndDate)>
            IsDateNotRecentAsync(DateTimeOffset date)
        {
            int pastSeconds = 60;
            int futureSeconds = 0;
            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            if (currentDateTime == default)
            {
                return (false, default, default);
            }

            TimeSpan timeDifference = currentDateTime.Subtract(date);
            DateTimeOffset startDate = currentDateTime.AddSeconds(-pastSeconds);
            DateTimeOffset endDate = currentDateTime.AddSeconds(futureSeconds);
            bool isNotRecent = timeDifference.TotalSeconds is > 60 or < 0;

            return (isNotRecent, startDate, endDate);
        }

        private static async ValueTask<dynamic> IsInvalidAsync(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is invalid"
        };

        private static async ValueTask<dynamic> IsInvalidAsync(string name) => new
        {
            Condition = String.IsNullOrWhiteSpace(name),
            Message = "Text is required"
        };

        private static async ValueTask<dynamic> IsInvalidAsync(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is invalid"
        };

        private static async ValueTask<dynamic> IsInvalidLengthAsync(string text, int maxLength) => new
        {
            Condition = await IsExceedingLengthAsync(text, maxLength),
            Message = $"Text exceed max length of {maxLength} characters"
        };

        private static async ValueTask<bool> IsExceedingLengthAsync(string text, int maxLength) =>
            (text ?? string.Empty).Length > maxLength;

        private static async ValueTask<dynamic> IsInvalidUrlAsync(string url) => new
        {
            Condition = await IsValidUrlAsync(url) is false,
            Message = "Url is invalid"
        };

        private static async ValueTask<dynamic> IsDatesNotSameAsync(
            DateTimeOffset createdDate,
            DateTimeOffset updatedDate,
            string createdDateName) => new
            {
                Condition = createdDate != updatedDate,
                Message = $"Date is not the same as {createdDateName}"
            };

        private static async ValueTask<dynamic> IsValuesNotSameAsync(
            string createBy,
            string updatedBy,
            string createdByName) => new
            {
                Condition = createBy != updatedBy,
                Message = $"Text is not the same as {createdByName}"
            };

        public static async ValueTask<bool> IsValidUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                return false;
            }

            return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
        }

        private static void ValidateSourceIsNotNull(Source source)
        {
            if (source is null)
            {
                throw new NullSourceException(message: "Source is null");
            }
        }

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidSourceException = new InvalidSourceException(
                message: "Source is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidSourceException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidSourceException.ThrowIfContainsErrors();
        }
    }
}