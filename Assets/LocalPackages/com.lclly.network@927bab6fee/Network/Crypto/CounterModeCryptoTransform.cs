using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Crypto
{
	public class CounterModeCryptoTransform : ICryptoTransform
	{
		private readonly byte[] _counter;
		private readonly byte[] _srcBytes;
		private readonly ICryptoTransform _counterEncryptor;
		private readonly Queue<byte> _xorMask = new Queue<byte>();
		private readonly SymmetricAlgorithm _symmetricAlgorithm;

		/// <summary>
		/// 是否重置技术
		/// </summary>
		private readonly bool isResetCount;

		public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] counter, bool isResetCount)
		{
			if (symmetricAlgorithm == null) throw new ArgumentNullException("symmetricAlgorithm");
			if (key == null) throw new ArgumentNullException("key");
			if (counter == null) throw new ArgumentNullException("counter");
			if (counter.Length != symmetricAlgorithm.BlockSize / 8)
				throw new ArgumentException(String.Format("Counter size must be same as block size (actual: {0}, expected: {1})",
					counter.Length, symmetricAlgorithm.BlockSize / 8));

			_symmetricAlgorithm = symmetricAlgorithm;
			var tmpKey = new byte[key.Length];
			var tmpCounter = new byte[counter.Length];
			Array.Copy(key, tmpKey, key.Length);
			Array.Copy(counter, tmpCounter, counter.Length);
			_counter = tmpCounter;

			//复制一份数据信息
			this.isResetCount = isResetCount;
			if(isResetCount)
            {
				_srcBytes = new byte[counter.Length];
				Array.Copy(_counter, 0, _srcBytes, 0, counter.Length);
            }

			var zeroIv = new byte[_symmetricAlgorithm.BlockSize / 8];
			_counterEncryptor = symmetricAlgorithm.CreateEncryptor(tmpKey, zeroIv);
		}

		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			var output = new byte[inputCount];
			TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
			return output;
		}

		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			for (var i = 0; i < inputCount; i++)
			{
				if (NeedMoreXorMaskBytes()) EncryptCounterThenIncrement();

				var mask = _xorMask.Dequeue();
				outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ mask);
			}

			//重置计数
			if(isResetCount)
				ResetCounter();

			return inputCount;
		}

		private bool NeedMoreXorMaskBytes()
		{
			return _xorMask.Count == 0;
		}

		private void EncryptCounterThenIncrement()
		{
			var counterModeBlock = new byte[_symmetricAlgorithm.BlockSize / 8];

			_counterEncryptor.TransformBlock(_counter, 0, _counter.Length, counterModeBlock, 0);
			IncrementCounter();

			foreach (var b in counterModeBlock)
			{
				_xorMask.Enqueue(b);
			}
		}

		private void IncrementCounter()
		{
			for (var i = _counter.Length - 1; i >= 0; i--)
			{
				if (++_counter[i] != 0)
					break;
			}
		}

		/// <summary>
		/// 重置计数数据
		/// </summary>
		private void ResetCounter()
        {
			//重置数组
			Array.Copy(_srcBytes, 0, _counter, 0, _srcBytes.Length);
			//重置mask
			_xorMask.Clear();

		}

		public int InputBlockSize { get { return _symmetricAlgorithm.BlockSize / 8; } }
		public int OutputBlockSize { get { return _symmetricAlgorithm.BlockSize / 8; } }
		public bool CanTransformMultipleBlocks { get { return true; } }
		public bool CanReuseTransform { get { return false; } }

		public void Dispose()
		{
		}
	}
}