using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChatUtilities
{
    public static class RectManagement
    {
        public static bool GetChatRect(out Rect chatRect)
        {
            chatRect = new Rect();

            if (Plugin.onlineChatUI == null)
            {                
                return false;
            }

            try
            {
                RectTransform chatboxBackgroundImage = Plugin.onlineChatUI.bigChatInput.GetComponent<Image>().rectTransform;
                Vector2 size = Vector2.Scale(chatboxBackgroundImage.rect.size, chatboxBackgroundImage.lossyScale);
                Rect screenSpace = new Rect((Vector2)chatboxBackgroundImage.position - (size * 0.5f), size);
                screenSpace.y = Screen.height - screenSpace.y - size.y;
                chatRect = screenSpace;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Rect GetRectUnderChat(Rect chatRect)
        {
            return new Rect(chatRect.position.x, chatRect.position.y + chatRect.size.y, chatRect.size.x, Screen.height - chatRect.position.y - chatRect.size.y);
        }

        public static Rect GetRectRightOfChat(Rect chatRect, int rectIndex)
        {
            return new Rect(chatRect.position.x + chatRect.size.x + rectIndex * chatRect.size.y, chatRect.position.y, chatRect.size.y, chatRect.size.y);
        }

        public static List<Rect> SubdivideRect(Rect rect, int columns, int rows)
        {
            List<Rect> rects = new List<Rect>();

            if(columns <= 0 || rows <= 0)
            {
                Debug.Log("Columns || Rows shouldnt be <= 0");
                rects.Add(rect);
                return rects;
            }

            float width = rect.width / columns;
            float height = rect.height / rows;

            for (int row = 0; row < rows; row++)
            {
                for(int col = 0; col < columns; col++)
                {
                    float x = rect.x + col * width;
                    float y = rect.y + row * height;
                    rects.Add(new Rect(x, y, width, height));
                }
            }

            return rects;
        }
    }
}
