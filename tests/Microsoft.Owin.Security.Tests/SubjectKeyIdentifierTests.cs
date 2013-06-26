// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class SubjectKeyIdentifierTests
    {
        private static readonly X509Certificate2 _SelfSigned = new X509Certificate2(Properties.Resources.SelfSignedCertificate);
        private static readonly X509Certificate2 _Chained = new X509Certificate2(Properties.Resources.ChainedCertificate);

        // The Katana test cert has a valid full chain
        // katanatest.redmond.corp.microsoft.com -> MSIT Machine Auth CA2 -> Microsoft Internet Authority -> Baltimore CyberTrustRoot

        private const string KatanaTestKeyIdentifier = "d964b2941aaf3e62761041b1f3db098edfa3270a";
        private const string MicrosoftInternetAuthorityKeyIdentifier = "2a4d97955d347e9db6e633be9c27c1707e67dbc1";

        [Fact]
        public void ConstructorShouldNotThrowWithValidValues()
        {
            var instance = new SubjectKeyIdentifierValidator(new[] {string.Empty});

            instance.ShouldNotBe(null);
        }

        [Fact]
        public void ConstructorShouldThrownWhenTheValidHashEnumerableIsNull()
        {
            Should.Throw<ArgumentNullException>(() =>
                new SubjectKeyIdentifierValidator(null));
        }

        [Fact]
        public void ValidatorShouldReturnFalseWhenSslPolicyErrorsIsRemoteCertificateChainErrors()
        {
            var instance = new SubjectKeyIdentifierValidator(new[] { string.Empty });
            var result = instance.RemoteCertificateValidationCallback(null, null, null, SslPolicyErrors.RemoteCertificateChainErrors);
            result.ShouldBe(false);
        }

        [Fact]
        public void ValidatorShouldReturnFalseWhenSslPolicyErrorsIsRemoteCertificateNameMismatch()
        {
            var instance = new SubjectKeyIdentifierValidator(new[] { string.Empty });
            var result = instance.RemoteCertificateValidationCallback(null, null, null, SslPolicyErrors.RemoteCertificateNameMismatch);
            result.ShouldBe(false);
        }

        [Fact]
        public void ValidatorShouldReturnFalseWhenSslPolicyErrorsIsRemoteCertificateNotAvailable()
        {
            var instance = new SubjectKeyIdentifierValidator(new[] { string.Empty });
            var result = instance.RemoteCertificateValidationCallback(null, null, null, SslPolicyErrors.RemoteCertificateNotAvailable);
            result.ShouldBe(false);
        }

        [Fact]
        public void ValidatorShouldReturnFalseWhenPassedASelfSignedCertificate()
        {
            var instance = new SubjectKeyIdentifierValidator(new[] { string.Empty });
            var certificateChain = new X509Chain();
            certificateChain.Build(_SelfSigned);
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            var result = instance.RemoteCertificateValidationCallback(null, _SelfSigned, certificateChain, SslPolicyErrors.None);

            result.ShouldBe(false);
        }

        [Fact]
        public void ValidatorShouldReturnFalseWhenPassedATrustedCertificateWhichDoesNotHaveAWhitelistedSubjectKeyIdentifier()
        {
            var instance = new SubjectKeyIdentifierValidator(new[] { string.Empty });            
            var certificateChain = new X509Chain();
            certificateChain.Build(_Chained);
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            var result = instance.RemoteCertificateValidationCallback(null, _Chained, certificateChain, SslPolicyErrors.None);

            result.ShouldBe(false);
        }

        [Fact]
        public void ValidatorShouldReturnTrueWhenPassedATrustedCertificateWhichHasItsSubjectKeyIdentifierWhiteListed()
        {
            var instance = new SubjectKeyIdentifierValidator(
                new[]
                {
                    KatanaTestKeyIdentifier
                });
            
            var certificateChain = new X509Chain();
            certificateChain.Build(_Chained);
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            var result = instance.RemoteCertificateValidationCallback(null, _Chained, certificateChain, SslPolicyErrors.None);

            result.ShouldBe(true);
        }

        [Fact]
        public void ValidatorShouldReturnTrueWhenPassedATrustedCertificateWhichHasAChainElementSubjectKeyIdentifierWhiteListed()
        {
            var instance = new SubjectKeyIdentifierValidator(
                new[]
                {
                    MicrosoftInternetAuthorityKeyIdentifier
                }); 
            var certificateChain = new X509Chain();
            certificateChain.Build(_Chained);
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            var result = instance.RemoteCertificateValidationCallback(null, _Chained, certificateChain, SslPolicyErrors.None);

            result.ShouldBe(true);
        }
    }
}
