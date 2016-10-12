using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleRESTServiceCRUD
{
    /// <summary>
    /// Gestiona la conversión de números.
    /// </summary>
    public static class Numeros
    {
        private enum TipoNumero
        {
            Boolean,
            Byte,
            Int16,
            Int32,
            Int64,
            UInt16,
            UInt32,
            UInt64,
            Single,
            Double,
            Decimal,
        }

        private static object ToNumber(TipoNumero tipoNumero, object valor, object defecto)
        {
            if (valor == null || (valor is DBNull)) { return defecto; }

            object numero = defecto;
            switch (tipoNumero)
            {
                case TipoNumero.Boolean:
                    if (valor is string)
                    {
                        Boolean numeroBoolean;
                        if (Boolean.TryParse((string)valor, out numeroBoolean))
                        { numero = numeroBoolean; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToBoolean(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.Byte:
                    if (valor is string)
                    {
                        Byte numeroByte;
                        if (Byte.TryParse((string)valor, out numeroByte))
                        { numero = numeroByte; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToByte(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.Int16:
                    if (valor is string)
                    {
                        Int16 numeroInt16;
                        if (Int16.TryParse((string)valor, out numeroInt16))
                        { numero = numeroInt16; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToInt16(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.Int32:
                    if (valor is string)
                    {
                        Int32 numeroInt32;
                        if (Int32.TryParse((string)valor, out numeroInt32))
                        { numero = numeroInt32; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToInt32(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.Int64:
                    if (valor is string)
                    {
                        Int64 numeroInt64;
                        if (Int64.TryParse((string)valor, out numeroInt64))
                        { numero = numeroInt64; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToInt64(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.UInt16:
                    if (valor is string)
                    {
                        UInt16 numeroUInt16;
                        if (UInt16.TryParse((string)valor, out numeroUInt16))
                        { numero = numeroUInt16; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToUInt16(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.UInt32:
                    if (valor is string)
                    {
                        UInt32 numeroUInt32;
                        if (UInt32.TryParse((string)valor, out numeroUInt32))
                        { numero = numeroUInt32; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToUInt32(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.UInt64:
                    if (valor is string)
                    {
                        UInt64 numeroUInt64;
                        if (UInt64.TryParse((string)valor, out numeroUInt64))
                        { numero = numeroUInt64; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToUInt64(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.Single:
                    if (valor is string)
                    {
                        Single numeroSingle;
                        if (Single.TryParse((string)valor, out numeroSingle))
                        { numero = numeroSingle; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToSingle(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.Double:
                    if (valor is string)
                    {
                        Double numeroDouble;
                        if (Double.TryParse((string)valor, out numeroDouble))
                        { numero = numeroDouble; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToDouble(valor); }
                        catch
                        {; }
                    }
                    break;
                case TipoNumero.Decimal:
                    if (valor is string)
                    {
                        Decimal numeroDecimal;
                        if (Decimal.TryParse((string)valor, out numeroDecimal))
                        { numero = numeroDecimal; }
                    }
                    else
                    {
                        try
                        { numero = Convert.ToDecimal(valor); }
                        catch
                        {; }
                    }
                    break;
            }

            return numero;
        }

        public static int ToInt(object valor)
        {
            return ToInt(valor, 0);
        }
        public static int ToInt(object valor, int defecto)
        {
            return (int)ToNumber(TipoNumero.Int32, valor, defecto);
        }

        public static long ToLong(object valor)
        {
            return ToLong(valor, 0);
        }
        public static long ToLong(object valor, long defecto)
        {
            return (long)ToNumber(TipoNumero.Int64, valor, defecto);
        }

        public static double ToDouble(object valor)
        {
            return ToDouble(valor, 0);
        }
        public static double ToDouble(object valor, double defecto)
        {
            return (double)ToNumber(TipoNumero.Double, valor, defecto);
        }

        public static decimal ToDecimal(object valor)
        {
            return ToDecimal(valor, 0);
        }
        public static decimal ToDecimal(object valor, decimal defecto)
        {
            return (decimal)ToNumber(TipoNumero.Decimal, valor, defecto);
        }

        public static float ToFloat(object valor)
        {
            return ToFloat(valor, 0);
        }
        public static float ToFloat(object valor, float defecto)
        {
            return (float)ToNumber(TipoNumero.Single, valor, defecto);
        }

        public static bool ToBool(object valor)
        {
            return ToBool(valor, false);
        }
        public static bool ToBool(object valor, bool defecto)
        {
            return (bool)ToNumber(TipoNumero.Boolean, valor, defecto);
        }

        private static Random rnd = null;
        public static Random Rnd
        {
            get { return rnd ?? (rnd = new Random(Guid.NewGuid().GetHashCode())); }
        }
        public static void Desordena(ref Object[] array, bool desordenar)
        {
            int rndIndex;
            int k = 0;
            for (int i = 0; i < array.Length; i++)
            {
                k++;
                array[i] = k;
            }
            if (desordenar)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        rndIndex = Convert.ToInt32(Math.Floor(Rnd.NextDouble() * array.Length));
                        Intercambia(ref array[i], ref array[rndIndex]);
                    }
                }
            }
        }
        public static void Desordena(ref Object[] array)
        {
            Desordena(ref array, true);
        }
        public static void Intercambia(ref int num1, ref int num2)
        {
            int tmpNum = num1;
            num1 = num2;
            num2 = tmpNum;
        }
        public static void Intercambia(ref object valor1, ref object valor2)
        {
            object tmpValor = valor1;
            valor1 = valor2;
            valor2 = tmpValor;
        }
    }
}