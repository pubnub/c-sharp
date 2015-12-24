// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Reflection;
using NUnit.Framework;

namespace NUnitLite
{
    public class Reflect
    {
        #region Types
        public static readonly Type[] EmptyTypes = new Type[0];

        public static bool IsTestCaseClass(Type type)
        {
            return typeof(TestCase).IsAssignableFrom(type);
        }

        /// <summary>
        /// Returns an array of types from an array of objects.
        /// Used because the compact framework doesn't support
        /// Type.GetTypeArray()
        /// </summary>
        /// <param name="objects">An array of objects</param>
        /// <returns>An array of Types</returns>
        public static Type[] GetTypeArray(object[] objects)
        {
            Type[] types = new Type[objects.Length];
            int index = 0;
            foreach (object o in objects)
                types[index++] = o.GetType();
            return types;
        }
        #endregion

        #region Interfaces

        /// <summary>
        /// Check to see if a type implements an interface.
        /// </summary>
        /// <param name="type">The type to examine</param>
        /// <param name="interfaceType">The interface type to check for</param>
        /// <returns>True if the interface is implemented by the type</returns>
        public static bool HasInterface(Type type, Type interfaceType)
        {
            // NOTE: IsAssignableForm fails so we look for the name
            return HasInterface( type, interfaceType.FullName );
        }

        /// <summary>
        /// Check to see if a type implements a named interface.
        /// </summary>
        /// <param name="fixtureType">The type to examine</param>
        /// <param name="interfaceName">The FullName of the interface to check for</param>
        /// <returns>True if the interface is implemented by the type</returns>
        public static bool HasInterface(Type fixtureType, string interfaceName)
        {
            foreach (Type type in fixtureType.GetInterfaces())
                if (type.FullName == interfaceName)
                    return true;
            return false;
        }
        #endregion

        #region Methods
        public static MethodInfo GetMethod(Type type, string name, params Type[] argTypes)
        {
            if (argTypes == null) argTypes = Reflect.EmptyTypes;
            return type.GetMethod(name, argTypes);
        }

        public static MethodInfo GetMethod(Type type, string name, BindingFlags flags, params Type[] argTypes)
        {
            if (argTypes == null) argTypes = Reflect.EmptyTypes;
            return type.GetMethod(name, flags, null, argTypes, null);
        }

        /// <summary>
        /// Invoke a method returning void on an object, converting any
        /// TargetInvocatonException to an NUnitException.
        /// </summary>
        /// <param name="method">A MethodInfo for the method to be invoked</param>
        /// <param name="fixture">The object on which to invoke the method</param>
        public static void InvokeMethod(MethodInfo method, object fixture, params object[] args)
        {
            if (method != null)
            {
                try
                {
                    method.Invoke(fixture, args);
                }
                catch (Exception e)
                {
                    if (e is TargetInvocationException)
                        throw new NUnitLiteException("Rethrown", e.InnerException);
                    else
                        throw new NUnitLiteException("Rethrown", e);
                }
            }
        }
        #endregion

        #region Construction
        public static bool HasConstructor(Type type, params Type[] argTypes)
        {
            return GetConstructor(type, argTypes) != null;
        }

        public static ConstructorInfo GetConstructor(Type type, params Type[] argTypes)
        {
            if (argTypes == null) argTypes = Reflect.EmptyTypes;
            return type.GetConstructor(argTypes);
        }

        public static object Construct(Type type, params object[] args)
        {
            Type[] argTypes;

            if (args == null)
            {
                args = new object[0];
                argTypes = Reflect.EmptyTypes;
            }
            else
            {
                argTypes = Reflect.GetTypeArray(args);
            }

            ConstructorInfo ctor = GetConstructor( type, argTypes );
            return ctor.Invoke(args);
        }

        public static ITest ConstructTestCase(MethodInfo method)
        {
            return ConstructTestCase(method, method.Name);
        }

        public static ITest ConstructTestCase(MethodInfo method, string name)
        {
            return ConstructTestCase(method.ReflectedType, name);
        }

        private static ITest ConstructTestCase(Type type, string name)
        {
            if (IsTestCaseClass(type))
                return (ITest)Construct( type, name );
            else
                return new ProxyTestCase( name, Construct( type ) );
        }
        #endregion

        #region Attributes
        public static bool HasAttribute(MemberInfo member, Type attr)
        {
            return member.GetCustomAttributes(attr, true).Length > 0;
        }

        public static Attribute GetAttribute(MemberInfo member, Type attrType)
        {
            object[] attrs = member.GetCustomAttributes(attrType, false);
            return attrs.Length == 0 ? null : attrs[0] as Attribute;
        }
        #endregion

        #region Properties
        public static PropertyInfo GetSuiteProperty(Type type)
        {
            return type.GetProperty("Suite", typeof(ITest), Reflect.EmptyTypes);
        }
        #endregion
    }
}
