using System.IO;
using System.Threading;

namespace Microsoft.Owin.Compression.Infrastructure
{
    public class SwitchingStream : DelegatingStream
    {
        private readonly StaticCompressionContext _compressingContext;
        private readonly Stream _originalBody;

        private Stream _targetStream;
        private bool _targetStreamInitialized;
        private object _targetStreamLock = new object();

        internal SwitchingStream(StaticCompressionContext compressingContext, Stream originalBody)
        {
            _compressingContext = compressingContext;
            _originalBody = originalBody;
        }

        protected override Stream TargetStream
        {
            get { return LazyInitializer.EnsureInitialized(
                ref _targetStream,
                ref _targetStreamInitialized,
                ref _targetStreamLock,
                _compressingContext.GetTargetStream);
            }
        }
    }
}
