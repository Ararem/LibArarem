﻿//Auto generated by a roslyn source generator

namespace LibEternal.Testing
{
	//Source type is LibEternal.Testing.StaticInstGenClass<T>
	///<inheritdoc cref="T:LibEternal.Testing.StaticInstGenClass`1"/>
	public static partial class Static<T>
			where T : notnull
	{
		/// <summary>
		///  A roslyn source-generator generated instance that will be used as the target for static instance members
		/// </summary>
		[System.Runtime.CompilerServices.CompilerGenerated]
		private static readonly LibEternal.Testing.StaticInstGenClass<T> Instance = new();

		///<inheritdoc cref="P:LibEternal.Testing.StaticInstGenClass`1.GetSetIntProperty"/>
		public static int GetSetIntProperty
		{
			get => Instance.GetSetIntProperty;
			set => Instance.GetSetIntProperty = value;
		}

		///<inheritdoc cref="F:LibEternal.Testing.StaticInstGenClass`1.IntField"/>
		public static int IntField
		{
			get => Instance.IntField;
			set => Instance.IntField = value;
		}

		///<inheritdoc cref="F:LibEternal.Testing.StaticInstGenClass`1.SystemRandomField"/>
		public static System.Random SystemRandomField
		{
			get => Instance.SystemRandomField;
			set => Instance.SystemRandomField = value;
		}

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.VoidMethod"/>
		public static void VoidMethod()
			=> Instance.VoidMethod();

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.ReturnsAString"/>
		public static string ReturnsAString()
			=> Instance.ReturnsAString();

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.F_Of_X(System.Int32,System.Single,System.Double,System.Int32)"/>
		public static double F_Of_X(int a, float b, double c, int x)
			=> Instance.F_Of_X(a, b, c, x);

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.ToBeOrNotToBe"/>
		public static bool ToBeOrNotToBe()
			=> Instance.ToBeOrNotToBe();

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.NotAnAsyncTask"/>
		public static System.Threading.Tasks.Task NotAnAsyncTask()
			=> Instance.NotAnAsyncTask();

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.AsyncVoid"/>
		public static void AsyncVoid()
			=> Instance.AsyncVoid();

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.AsyncTask"/>
		public static async System.Threading.Tasks.Task AsyncTask()
			=> await Instance.AsyncTask();

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.GenericAsyncTask``1(``0)"/>
		public static async System.Threading.Tasks.Task<T> GenericAsyncTask<T>(T t)
			=> await Instance.GenericAsyncTask<T>(t);

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.GenericAsyncWithConstraints``1"/>
		public static async System.Threading.Tasks.Task GenericAsyncWithConstraints<T>()
			where T : class, System.Collections.Generic.IEnumerable<T>, new()
			=> await Instance.GenericAsyncWithConstraints<T>();

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.Simple``1(``0)"/>
		public static T Simple<T>(T t)
			=> Instance.Simple<T>(t);

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.OneComplexParameter``1(``0)"/>
		public static T OneComplexParameter<T>(T t)
			where T : class, System.Collections.IEnumerable, new()
			=> Instance.OneComplexParameter<T>(t);

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.ComplexParams``2(``1)"/>
		public static T1 ComplexParams<T1, T2>(T2 t2)
			where T1 : class, System.Collections.Generic.IEnumerable<T1>, new()
			where T2 : unmanaged, System.IEquatable<System.Collections.Generic.IEnumerable<System.Collections.Generic.Dictionary<int, string>>>
			=> Instance.ComplexParams<T1, T2>(t2);

		///<inheritdoc cref="M:LibEternal.Testing.StaticInstGenClass`1.VeryComplexInheritance``2(``0,``1)"/>
		public static TBase VeryComplexInheritance<TBase, TInherited>(TBase _base, TInherited inherited)
			where TBase : notnull, System.Delegate
			where TInherited : class, TBase, System.Collections.Generic.IEnumerable<TBase>, System.IComparable<TInherited>, System.Collections.IEnumerable
			=> Instance.VeryComplexInheritance<TBase, TInherited>(_base, inherited);

		///<inheritdoc cref="F:LibEternal.Testing.StaticInstGenClass`1.ReadonlyField"/>
		public static int ReadonlyField => Instance.ReadonlyField;

		///<inheritdoc cref="P:LibEternal.Testing.StaticInstGenClass`1.GetOnly"/>
		public static int GetOnly
		{
			get => Instance.GetOnly;
		}

		///<inheritdoc cref="P:LibEternal.Testing.StaticInstGenClass`1.SetOnly"/>
		public static int SetOnly
		{
			set => Instance.SetOnly = value;
		}

		///<inheritdoc cref="P:LibEternal.Testing.StaticInstGenClass`1.GetInit"/>
		public static int GetInit
		{
			get => Instance.GetInit;
		}
	} //End class
} //End namespace