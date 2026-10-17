// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace MeaMod.Utilities.Security
{
    /// <summary>
    /// Helper fns.
    /// </summary>
    internal static class Utils
    {

        /// <summary>
        /// Added to support missing TryAdd in net48
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper fn to check arg for empty or null.
        /// Throws ArgumentNullException on either condition.
        /// </summary>
        /// <param name="arg"> arg to check </param>
        /// <param name="argName"> name of the arg </param>
        /// <returns> Does not return a value.</returns>
        internal static void CheckArgForNullOrEmpty(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
            else if (arg.Length == 0)
            {
                throw new ArgumentNullException(argName);
            }
        }

    }
}
