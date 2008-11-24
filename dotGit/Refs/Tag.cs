﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using dotGit.Objects.Storage;
using dotGit.Objects;
using IO = System.IO;
using dotGit.Objects;
using dotGit.Exceptions;
using dotGit.Generic;

namespace dotGit.Refs
{
	/// <summary>
	/// Represents a tag in the repository. Tags can reference any IStorableObject in the repository
	/// </summary>
	public class Tag : Ref, IStorableObject
	{
		internal Tag(Repository repo)
			: base(repo)
		{ }

		internal Tag(Repository repo, string sha)
			: this(repo)
		{
			SHA = sha;
		}

		/// <summary>
		/// The SHA this tag is referenced by
		/// </summary>
		public string SHA
		{
			get;
			private set;
		}

		internal static Tag GetTag(Repository repo, string path)
		{
			IO.FileInfo refFile = Ref.GetRefFile(repo, path);

			string sha = IO.File.ReadAllText(refFile.FullName).Trim();
			if (!Utility.IsValidSHA(sha))
				throw new ParseException("Tag does not contain valid sha");


			IStorableObject obj = repo.Storage.GetObject(sha);


			// This is kind of flaky but for now the only way we can distinguish a regular and a lightweight tag
			if (obj is Tag)
			{
				((Tag)obj).Path = path;
				return (Tag)obj;
			}
			else
			{
				Tag t = new Tag(repo, sha);
				t.Object = obj;
				t.Path = path;
				return t;
			}
		}


		/// <summary>
		/// The IStorableObject this tag references
		/// </summary>
		public IStorableObject Object
		{
			get;
			private set;
		}

		/// <summary>
		/// The contributer that made this tag
		/// </summary>
		public Contributer Tagger
		{
			get;
			private set;
		}


		/// <summary>
		/// The date this tag was made
		/// </summary>
		public DateTime TagDate
		{
			get;
			private set;
		}

		/// <summary>
		/// The message for this tag
		/// </summary>
		public string Message
		{
			get;
			private set;
		}

		/// <summary>
		/// Returns true if this tag has a message. Returns false if this is a 'lightweight' tag
		/// </summary>
		public bool IsAnnotated
		{
			get { return !String.IsNullOrEmpty(Message) && Tagger != null && TagDate != null; }
		}


		/// <summary>
		/// Loads the tag from the GitObjectReader
		/// </summary>
		/// <param name="input">A reader with inflated tag contents</param>
		public void Deserialize(GitObjectReader input)
		{
			if (String.IsNullOrEmpty(SHA))
				SHA = Sha.Compute(input);


			string sha;
			if (Utility.IsValidSHA(input.GetString(20), out sha))
			{ // Tag contains a regular SHA so we can assume it's a commit
				Object = Repo.Storage.GetObject(sha);
				return;
			}
			else
			{
				input.Rewind();

				// Skip header
				input.ReadToNull();

				// Skip object keyword
				input.ReadWord();

				string objectSha;
				objectSha = input.ReadLine().GetString().Trim();
				if (!Utility.IsValidSHA(objectSha))
					throw new ParseException("Invalid sha from tag content");

				// Load object; a ParseException will be thrown for unknown types
				Object = Repo.Storage.GetObject(objectSha);

				// Skip type and tag
				input.ReadLine(); input.ReadLine();

				// Tagger
				input.ReadWord();
				string taggerLine = input.ReadLine().GetString();
				TagDate = Utility.StripDate(taggerLine, out taggerLine);
				Tagger = Contributer.Parse(taggerLine);

				//Skip extra '\n' and read message
				input.ReadBytes(1);
				Message = input.ReadToEnd().GetString().TrimEnd();

			}
		}

		public byte[] Serialize()
		{
			throw new NotImplementedException();
		}
	}
}
