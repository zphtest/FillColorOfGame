using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDemo : MonoBehaviour
{
    public static Color Pen_Colour = Color.red;
    public static int Pen_Width = 3;

    public LayerMask Drawing_Layers;

    private Sprite drawable_sprite;
    private Texture2D drawable_texture;

    private Vector2 previous_drag_position;
    private Color[] clean_colours_array;
    private Collider2D[] rayResult = new Collider2D[2];
    private Color32[] cur_colors;

    private bool no_drawing_on_current_drag = false;
    private bool mouse_was_previously_held_down = false;

    void Awake()
    {
        drawable_sprite = this.GetComponent<SpriteRenderer>().sprite;
        drawable_texture = drawable_sprite.texture;

        clean_colours_array = new Color[(int)drawable_sprite.rect.width * (int)drawable_sprite.rect.height];
        clean_colours_array = drawable_texture.GetPixels();
    }

    void Update()
    {
        bool mouse_held_down = Input.GetMouseButton(0);
        if (mouse_held_down && !no_drawing_on_current_drag)
        {
            Vector2 mouse_world_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D hit = Physics2D.OverlapPoint(mouse_world_position, Drawing_Layers.value);
            if (hit != null && hit.transform != null)
            {
                PenBrush(mouse_world_position);
                //current_brush(mouse_world_position);
            }
            else
            {
                previous_drag_position = Vector2.zero;
                if (!mouse_was_previously_held_down)
                {
                    no_drawing_on_current_drag = true;
                }
            }
        }
        else if (!mouse_held_down)
        {
            previous_drag_position = Vector2.zero;
            no_drawing_on_current_drag = false;
        }
        mouse_was_previously_held_down = mouse_held_down;
    }

    protected void OnDestroy()
    {
        ResetCanvas();
    }

    /// <summary>
    ///  ÷ÿ÷√ª≠≤º
    /// </summary>
    private void ResetCanvas()
    {
        drawable_texture.SetPixels(clean_colours_array);
        drawable_texture.Apply();
    }

    /// <summary>
    ///  ± À¢
    /// </summary>
    public void PenBrush(Vector2 world_point)
    {
        Vector2 pixel_pos = WorldToPixelCoordinates(world_point);

        cur_colors = drawable_texture.GetPixels32();

        if (previous_drag_position == Vector2.zero)
        {
            MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
        }
        else
        {
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
        }
        ApplyMarkedPixelChanges();

        previous_drag_position = pixel_pos;
    }

    private Vector2 WorldToPixelCoordinates(Vector2 world_position)
    {
        Vector3 local_pos = transform.InverseTransformPoint(world_position);

        float pixelWidth = drawable_sprite.rect.width;
        float pixelHeight = drawable_sprite.rect.height;
        float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

        return pixel_pos;
    }

    private void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
    {
        float distance = Vector2.Distance(start_point, end_point);
        Vector2 direction = (start_point - end_point).normalized;

        Vector2 cur_position = start_point;
        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }

    private void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            if (x >= (int)drawable_sprite.rect.width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen);
            }
        }
    }
    private void MarkPixelToChange(int x, int y, Color color)
    {
        int array_pos = y * (int)drawable_sprite.rect.width + x;

        if (array_pos > cur_colors.Length || array_pos < 0)
            return;

        cur_colors[array_pos] = color;
    }

    private void ApplyMarkedPixelChanges()
    {
        drawable_texture.SetPixels32(cur_colors);
        drawable_texture.Apply();
    }
}