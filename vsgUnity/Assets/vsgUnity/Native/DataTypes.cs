//----------------------------------------------
//            vsgUnity: Native
// Writen by Thomas Hogarth
// DataTypes.cs
//----------------------------------------------

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vsgUnity.Native
{

    //
    // Local Unity types, should match layout of types in unity2vg DataTypes.h, used to pass data from C# to native code
    //

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
	public struct IntArray
	{
		public int[] data;
		public uint length;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
	public struct FloatArray
	{
		public float[] data;
		public uint length;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
	public struct Vec2Array
	{
		public Vector2[] data;
		public uint length;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
    public struct Vec3Array
    {
        public Vector3[] data;
        public uint length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
    public struct Vec4Array
    {
        public Vector4[] data;
        public uint length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
    public struct Mesh
    {
        public Vec3Array verticies;
        public IntArray triangles;
        public Vec3Array normals;
        public Vec2Array uv0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
    public struct MeshArray
    {
        public Mesh[] data;
        public uint length;
    }

    [Serializable]
    public enum NodeType
    {
        GROUP = 0,
        TRANSFORM = 1,
        MESH = 2,
        LIGHT = 3
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
    public struct SceneNodeArray
    {
        public SceneNode[] data;
        public uint length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
    public struct SceneNode
    {
        //public string name;

        public uint type;

        public SceneNodeArray children;

        // transform data
        public FloatArray matrix; // 16 element array for matrix data

        // mesh data
        public uint meshID;

        // light data

    };

    [StructLayout(LayoutKind.Sequential, Pack = 0), Serializable]
    public struct ExportScene
    {
        public SceneNode root;

        public MeshArray meshes;

        //public string exportPath;
    };

    //
    // Native types for data returned from native code to C#
    //

    [StructLayout(LayoutKind.Sequential)]
	public struct NativeIntArray
	{
		public IntPtr ptr;
		public uint length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NativeFloatArray
	{
		public IntPtr ptr;
		public uint length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NativeVec2Array
	{
		public IntPtr ptr;
		public uint length;
	}

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec3Array
    {
        public IntPtr ptr;
        public uint length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec4Array
    {
        public IntPtr ptr;
        public uint length;
    }

    public static class Memory
	{
#if UNITY_IPHONE
		[DllImport ("__Internal")]
#else
        [DllImport("unity2vsgd", EntryPoint = "unity2vsg_DataTypes_DeleteNativeObject")]
#endif
        private static extern void unity2vsg_DataTypes_DeleteNativeObject(IntPtr anObjectPointer, bool isArray);

		public static void DeleteNativeObject(IntPtr anObjectPointer, bool isArray)
		{
            unity2vsg_DataTypes_DeleteNativeObject(anObjectPointer, isArray);
		}
	}

    public static class Convert
	{
	 	private static T[] CreateArray<T>(IntPtr array, uint length)
		{
	         T[] result = new T[length];
	         int size = Marshal.SizeOf(typeof(T));
	 
	         if (IntPtr.Size == 4) {
	             // 32-bit system
	             for (int i = 0; i < result.Length; i++) {
	                 result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
	                 array = new IntPtr (array.ToInt32 () + size);
	             }
	         } else {
	             // probably 64-bit system
	             for (int i = 0; i < result.Length; i++) {
	                 result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
	                 array = new IntPtr(array.ToInt64 () + size);
	             }
	         }
	         return result;
     	}

		public static IntArray FromLocal(int[] anArray)
		{
			IntArray result;
			result.data = anArray;
			result.length = (uint)anArray.Length;
			return result;
		}

		public static FloatArray FromLocal(float[] anArray)
		{
			FloatArray result;
			result.data = anArray;
			result.length = (uint)anArray.Length;
			return result;
		}

		public static Vec2Array FromLocal(Vector2[] anArray)
		{
			Vec2Array result;
			result.data = anArray;
			result.length = (uint)anArray.Length;
			return result;
		}

        public static Vec3Array FromLocal(Vector3[] anArray)
        {
            Vec3Array result;
            result.data = anArray;
            result.length = (uint)anArray.Length;
            return result;
        }

        public static Vec4Array FromLocal(Vector4[] anArray)
        {
            Vec4Array result;
            result.data = anArray;
            result.length = (uint)anArray.Length;
            return result;
        }

        public static IntArray FromNative(NativeIntArray aNativeArray)
		{
			IntArray result;
			result.data = CreateArray<int>(aNativeArray.ptr, aNativeArray.length);
			result.length = (uint)result.data.Length;
			return result;
		}

		public static FloatArray FromNative(NativeFloatArray aNativeArray)
		{
			FloatArray result;
			result.data = CreateArray<float>(aNativeArray.ptr, aNativeArray.length);
			result.length = (uint)result.data.Length;
			return result;
		}

		public static Vec2Array FromNative(NativeVec2Array aNativeArray)
		{
			Vec2Array result;
			result.data = CreateArray<Vector2>(aNativeArray.ptr, aNativeArray.length);
			result.length = (uint)result.data.Length;
			return result;
		}

        public static Vec3Array FromNative(NativeVec3Array aNativeArray)
        {
            Vec3Array result;
            result.data = CreateArray<Vector3>(aNativeArray.ptr, aNativeArray.length);
            result.length = (uint)result.data.Length;
            return result;
        }

        public static Vec4Array FromNative(NativeVec4Array aNativeArray)
        {
            Vec4Array result;
            result.data = CreateArray<Vector4>(aNativeArray.ptr, aNativeArray.length);
            result.length = (uint)result.data.Length;
            return result;
        }
    }

}
