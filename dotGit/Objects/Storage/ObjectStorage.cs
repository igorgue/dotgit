﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using dotGit.Exceptions;
using dotGit.Generic;
using dotGit.Refs;

namespace dotGit.Objects.Storage
{

	/// <summary>
	/// Gateway to all of Git's stored objects. Either loose or packed
	/// </summary>
	public class ObjectStorage
	{
		private ObjectStorage()
		{ }

		/// <summary>
		/// Instantiates a new ObjectStorage object. It needs a reference to a repository object to operate
		/// </summary>
		/// <param name="repo">The repository to work with</param>
		public ObjectStorage(Repository repo)
		{
			Repo = repo;

			string[] indexFiles = Directory.GetFiles(Path.Combine(ObjectsDir, "pack"));

			// strip file extension and collect unique pack names
			indexFiles = indexFiles.Select((content) => Path.Combine(Path.GetDirectoryName(content), Path.GetFileNameWithoutExtension(content))).Distinct().ToArray();
			Packs = new InternalWritableList<Pack>(indexFiles.Length);
			
			foreach (string packFile in indexFiles)
			{
				Packs.Add(new Pack(packFile));
			}
		}

		internal InternalWritableList<Pack> Packs
		{
			get;
			private set;
		}

		/// <summary>
		/// The full path to repositories' objects directory
		/// </summary>
		public string ObjectsDir
		{
			get
			{
				return Path.Combine(Repo.GitDir.FullName, "objects");
			}
		}

		private Repository Repo { get; set; }

		/// <summary>
		/// Find object in database and return it as an IStorableObject. Use the generic overload if you know the type up front
		/// </summary>
		/// <param name="sha">SHA object identifier</param>
		/// <returns>GitObject from </returns>
		public IStorableObject GetObject(string sha)
		{
			if (!Utility.SHAExpression.IsMatch(sha))
				throw new ArgumentException("Need a valid sha", "sha");


			string looseObjectPath = Path.Combine(ObjectsDir, Path.Combine(sha.Substring(0, 2), sha.Substring(2)));
			// First check if object is stored in loose format
			if (File.Exists(looseObjectPath))
			{ // Object is stored loose. Inflate and load it from content
				using (GitObjectReader reader = new GitObjectReader(Zlib.Decompress(looseObjectPath)))
				{
					return LoadObjectFromInflatedStream(Repo, reader, sha);
				}
			}
			else
			{
				// TODO: Look for object in pack files
			}


			// Object was not found
			throw new ObjectNotFoundException(sha);
		}

		/// <summary>
		/// Use this if you already know the objects type
		/// </summary>
		/// <typeparam name="T">The type of object to fetch from the db. IStorableObject must be implemented</typeparam>
		/// <param name="sha"></param>
		/// <returns></returns>
		public T GetObject<T>(string sha) where T : IStorableObject
		{
			// For now we're just casting it to the given type
			return (T)GetObject(sha);
		}

		private static IStorableObject LoadObjectFromInflatedStream(Repository repo, GitObjectReader input, string sha)
		{
			long length;
			string type;
			length = input.ReadObjectHeader(out type);
			

			bool haveSha = !String.IsNullOrEmpty(sha);

			// If sha is passed we can forward it to the object so the SHA does not have to be calculated from the objects contents
			if (haveSha && !Utility.IsValidSHA(sha))
				throw new ArgumentException("Must have valid sha", "sha");

			IStorableObject result;
			switch (type)
			{
				case "commit":
					if (haveSha)
						result = new Commit(repo, sha);
					else
						result = new Commit(repo);
					break;
				case "tree":
					if (haveSha)
						result = new Tree(repo, sha);
					else
						result = new Tree(repo);
					break;
				case "blob":
					if (haveSha)
						result = new Blob(repo, sha);
					else
						result = new Blob(repo);
					break;
				case "tag":
					if (haveSha)
						result = new Tag(repo, sha);
					else
						result = new Tag(repo);
					break;
				default:
					throw new ParseException(String.Format("Could not open object of type: {0}", type));
			}

			// Let the respective object type load itself from the object content
			result.Deserialize(input);

			return result;
		}

	}
}
