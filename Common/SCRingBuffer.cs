using System;
using System.Collections;
using System.Collections.Generic;

namespace SC
{
    public class SCRingBuffer
    {
        public SCRingBuffer(int size_)
		{
            _capacity = size_;
            _front = 0;
            _rear = 0;
            _buffer = new byte[_capacity];
            _tempCopyBuffer = new byte[1024];
        }

        private int _capacity;
        private int _front;
        private int _rear;

        private byte[] _buffer;
        private byte[] _tempCopyBuffer;

        public int Size()
		{
            if (_front == _rear)
                return 0;
            else if (_rear > _front)
                return (_rear - _front);
            else if (_front > _rear)
                return (Capacity() - _front + _rear);

            return 0;
		}

        public int Capacity()
		{
            return _capacity;
		}

        public void Push(byte[] src_, int size_)
		{
            Prep(size_);

            if ((_rear + size_) < Capacity())
			{
                System.Buffer.BlockCopy(src_, 0, _buffer, _rear, size_);

                _rear += size_;
            }
            else
			{
                int offset = Capacity() - _rear;
                System.Buffer.BlockCopy(src_, 0, _buffer, _rear, offset);
                System.Buffer.BlockCopy(src_, offset, _buffer, 0, size_ - offset);

                _rear = size_ - offset;
            }
		}

        public bool Pop(byte[] dest_, int size_)
		{
            return Pop(dest_, size_, false);
        }

        public bool Read(byte[] dest_, int size_)
		{
            return Pop(dest_, size_, true);
		}

        private bool Pop(byte[] dest_, int size_, bool isReadonly_)
        {
            if (Size() <= 0 || Size() < size_)
            {
                return false;
            }

            if (_rear > _front)
            {
                System.Buffer.BlockCopy(_buffer, _front, dest_, 0, size_);
            }
            else
            {
                int offset = Capacity() - _front;
                if (offset >= size_)
                {
                    System.Buffer.BlockCopy(_buffer, _front, dest_, 0, offset);
                }
                else
                {
                    System.Buffer.BlockCopy(_buffer, _front, dest_, 0, offset);
                    System.Buffer.BlockCopy(_buffer, 0, dest_, offset, size_ - offset);
                }
            }

            if(isReadonly_ == false)
                _front = (_front + size_) % Capacity();

            return true;
        }

        private void Prep(int size_)
		{
            while(Size() + size_ >= Capacity())
			{
                byte[] newBuffer = new byte[Capacity() * 2];

                if (_rear >= _front)
                {
                    System.Buffer.BlockCopy(_buffer, _front, newBuffer, 0, Size());
                }
                else//_front > _rear
				{
                    System.Buffer.BlockCopy(_buffer, _front, newBuffer, 0, Capacity() - _front);
                    System.Buffer.BlockCopy(_buffer, 0, newBuffer, Capacity() - _front, _rear);
                }

                _rear = Size();
                _front = 0;

                _buffer = newBuffer;
                _capacity = _buffer.Length;
            }
		}
    }
}
