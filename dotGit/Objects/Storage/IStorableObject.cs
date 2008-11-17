﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotGit.Objects.Storage
{
	public interface IStorableObject
	{
		/// <summary>
		/// Load object contents from byte array
		/// </summary>
		/// <param name="contents"></param>
		void Deserialize(byte[] contents);

		/// <summary>
		/// Serializes the object to an array of bytes
		/// </summary>
		/// <returns></returns>
		byte[] Serialize();

		/// <summary>
		/// The SHA the object is identified by
		/// </summary>
		string SHA { get; }
	}
}