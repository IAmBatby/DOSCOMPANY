using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DOSCOMPANY
{
    public static class MatUtils
    {

        public static bool TryGetColor(this Material material, int property, out Color color)
        {
            color = Color.black;
            if (property == -1) return (false);
            if (material.HasColor(property))
            {
                color = material.GetColor(property);
                return (true);
            }
            return (false);
        }


        public static bool TrySetColor(this Material material, int property, Color color)
        {
            if (property == -1) return (false);
            if (material.HasColor(property))
            {
                material.SetColor(property, color);
                return (true);
            }
            return (false);
        }

        public static bool TryGetTexture(this Material material, int property, out Texture texture)
        {
            texture = null;
            if (property == -1) return (false);
            if (material.HasTexture(property))
            {
                texture = material.GetTexture(property);
                if (texture != null)
                    return (true);
                else
                    return (false);
            }
            return (false);
        }


        public static bool TryGetTexture2D(this Material material, int property, out Texture2D texture)
        {
            texture = null;
            if (property == -1) return (false);
            if (material.HasTexture(property))
            {
                Texture textureResult = material.GetTexture(property);
                if (textureResult != null && textureResult is Texture2D texture2D)
                {
                    texture = texture2D;
                    return (true);
                }
                else
                    return (false);
            }
            return (false);
        }


        public static bool TrySetTexture(this Material material, int property, Texture texture)
        {
            if (property == -1) return (false);
            if (material.HasTexture(property))
            {
                material.SetTexture(property, texture);
                return (true);
            }
            return (false);
        }

        public static bool TryGetFloat(this Material material, int property, out float floatValue)
        {
            floatValue = -1f;
            if (property == -1) return (false);
            if (material.HasFloat(property))
            {
                floatValue = material.GetFloat(property);
                return (true);
            }
            return (false);
        }


        public static bool TrySetFloat(this Material material, int property, float floatValue)
        {
            if (property == -1) return (false);
            if (material.HasFloat(property))
            {
                material.SetFloat(property, floatValue);
                return (true);
            }
            return (false);
        }
    }
}
