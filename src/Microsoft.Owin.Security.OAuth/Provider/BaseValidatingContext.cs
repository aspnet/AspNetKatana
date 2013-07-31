// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public abstract class BaseValidatingContext<TOptions> : BaseContext<TOptions>
    {
        protected BaseValidatingContext(
            IOwinContext context,
            TOptions options)
            : base(context, options)
        {
        }

        public bool IsValidated { get; private set; }

        public bool HasError { get; private set; }
        public string Error { get; private set; }
        public string ErrorDescription { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "error_uri is a string value in the protocol")]
        public string ErrorUri { get; private set; }

        public virtual bool Validated()
        {
            IsValidated = true;
            HasError = false;
            return true;
        }

        public virtual void Rejected()
        {
            IsValidated = false;
            HasError = false;
        }

        public void SetError(string error)
        {
            SetError(error, null);
        }

        public void SetError(string error,
            string errorDescription)
        {
            SetError(error, errorDescription, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "error_uri is a string value in the protocol")]
        public void SetError(string error,
            string errorDescription,
            string errorUri)
        {
            Error = error;
            ErrorDescription = errorDescription;
            ErrorUri = errorUri;
            Rejected();
            HasError = true;
        }
    }
}
