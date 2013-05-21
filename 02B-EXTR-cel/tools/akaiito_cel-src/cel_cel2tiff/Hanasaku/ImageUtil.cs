using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Hanasaku
{
    /// <summary>
    /// A compilation of standalone (static) functions related to image manipulation.
    /// </summary>
    static class ImageUtil
    {
        #region Class constants
        /// <summary>
        /// A GDI+ Property Tag ID, used to store vendor-specific data.
        /// See <a href="http://msdn.microsoft.com/en-us/library/ms534416.aspx#_gdiplus_constant_propertytagexifmakernote">http://msdn.microsoft.com/en-us/library/ms534416.aspx</a>
        /// Used in <a href="http://msdn.microsoft.com/en-us/library/system.drawing.imaging.propertyitem.id.aspx">PropertyItem.Id</a>
        /// such as in the <see cref="ImageUtil.Image_SetPropertyItemAscii"/> function.
        /// </summary>
        public readonly static UInt16 PropertyTagExifMakerNote = 0x927C;
        #endregion

        /// <summary>
        /// Obtains an <see cref="System.Drawing.Imaging.ImageCodecInfo"/> object corresponding to the specified <paramref name="mimeType"/>.
        /// </summary>
        /// <remarks>
        /// Copied from MSDN.
        /// See also <a href="http://msdn.microsoft.com/en-us/library/bb882579.aspx">http://msdn.microsoft.com/en-us/library/bb882579.aspx</a>
        /// </remarks>
        /// <example>
        /// <code>
        /// ImageCodecInfo encoderInfo = ImageUtil.GetEncoderInfo("image/tiff");
        /// </code>
        /// </example>
        /// <param name="mimeType">A string for the codec's Multipurpose Internet Mail Extensions (MIME) type.</param>
        /// <returns>an object reference if the requested codec is implemented, <value>null</value> otherwise.</returns>
        public static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        /// <summary>
        /// Sets a <paramref name="propId">property</paramref> (metadata) on a <see cref="System.Drawing.Image"/> object.
        /// Its <paramref name="propValue">value</paramref> is set as text using the <see cref="System.Text.Encoding.ASCII">ASCII encoding</see>.
        /// </summary>
        /// <param name="image">A non-<value>null</value> reference</param>
        /// <param name="propId">An integer corresponding to an <a href="http://msdn.microsoft.com/en-us/library/system.drawing.imaging.propertyitem.id.aspx">image property</a>.
        /// </param>
        /// <param name="propValue">A string containing the property value. It is forcefully mapped to ASCII.</param>
        public static void Image_SetPropertyItemAscii(ref Image image, int propId, string propValue)
        {
            try
            {
                PropertyItem pi = image.GetPropertyItem(propId);
                System.Text.ASCIIEncoding textConverter = new System.Text.ASCIIEncoding();

                // Set the value for an existing PropertyItem with 
                // ID == propId
                pi.Value = textConverter.GetBytes(propValue);
                pi.Len = pi.Value.Length;
                image.SetPropertyItem(pi);
            }
            catch (ArgumentException)
            {
                // Create a new PropertyItem when such an item does not exist.
                image.SetPropertyItem(Image_CreateNewPropertyItemAscii(propId, propValue));
            }
        }

        /// <summary>
        /// The .NET interface for GDI+ does not allow instantiation of the
        /// PropertyItem class. Therefore one must be stolen off the Image
        /// and repurposed.  GDI+ uses PropertyItem by value so there is no
        /// side effect when changing the values and reassigning to the image.
        /// </summary>
        /// <remarks>
        /// Copied from MSDN communities.
        /// </remarks>
        /// <param name="propId">An integer corresponding to an <a href="http://msdn.microsoft.com/en-us/library/system.drawing.imaging.propertyitem.id.aspx">image property</a>.
        /// </param>
        /// <param name="propValue">A string containing the property value. It is forcefully mapped to ASCII.</param>
        /// <returns>An initialized <see cref="System.Drawing.Imaging.PropertyItem"/> object assigned with <value>PropertyTagTypeASCII</value></returns>
        public static PropertyItem Image_CreateNewPropertyItemAscii(int propId, string propValue)
        {
            short propType = 2;  // PropertyTagTypeASCII
            System.Text.ASCIIEncoding textConverter = new System.Text.ASCIIEncoding();

            // About any image is bound to have PropertyItem's
            using (MemoryStream ms = new MemoryStream(
                Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQAAAADsdIMmAAAADElEQVR42mP4z4ACAT/QB/ln" +
                    "LUmhAAAAAElFTkSuQmCC"
                )
            ))
            {
                Bitmap dummyBitmapWithProps = new Bitmap(ms);
                PropertyItem pi = dummyBitmapWithProps.PropertyItems[0];
                pi.Id = propId;
                pi.Type = propType;
                pi.Value = textConverter.GetBytes(propValue);
                pi.Len = pi.Value.Length;  // Theorically, ASCIIZ, including \x00
                return pi;
            }
        }


        /// <summary>
        /// Sets a <paramref name="propId">property</paramref> (metadata) on a <see cref="System.Drawing.Bitmap"/> object.
        /// Its <paramref name="propValue">value</paramref> is set as text using the <see cref="System.Text.Encoding.ASCII">ASCII encoding</see>.
        /// </summary>
        /// <param name="image">A non-<value>null</value> reference</param>
        /// <param name="propId">An integer corresponding to an <a href="http://msdn.microsoft.com/en-us/library/system.drawing.imaging.propertyitem.id.aspx">image property</a>.
        /// </param>
        /// <returns>the property value, or null if it is not set</returns>
        public static string Image_GetPropertyItemAscii(Image image, int propId)
        {
            try
            {
                PropertyItem pi = image.GetPropertyItem(propId);
                return System.Text.ASCIIEncoding.ASCII.GetString(pi.Value);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
