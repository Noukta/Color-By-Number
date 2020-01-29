using UnityEngine;
using System.Collections.Generic;

public class MedianCut
{
    public Color32[] data;
    public class BBox
    {
        public Color32[] data;
        public List<int> indexes;
        public Chanel[] chanels = new Chanel[3];
        public int area;
        public int max;
        public Color32 centerColor;
        public bool cannotCut;
    }

    public class Chanel
    {
        public int count;
        public int min = 255;
        public int max = 0;
        public int distance;
    }

    public List<BBox> DoTask(Color32[] data, int n)
    {
        List<BBox> boxes = new List<BBox>
        {
            GetBoundingBox(data)
        };

        if (n != 1)
        {
            boxes = Cut(boxes[0]);
            while (boxes.Count < n)
            {
                var boxId = FindBiggestIndex(boxes);
                var splittedBoxes = Cut(boxes[boxId]);

                if (splittedBoxes != null)
                {
                    boxes.RemoveAt(boxId);
                    boxes.Insert(boxId, splittedBoxes[0]);
                    boxes.Insert(boxId + 1, splittedBoxes[1]);
                }
            }
        }

        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].centerColor = GetCenterColor(boxes[i]);
        };

        return boxes;
    }

    private int FindBiggestIndex(List<BBox> boxes)
    {
        var biggest = 0;
        var id = 0;

        for (var i = 0; i < boxes.Count; i++)
        {
            if (!boxes[i].cannotCut && boxes[i].area > biggest)
            {
                biggest = boxes[i].area;
                id = i;
            }
        };
        return id;
    }

    private Color32 GetCenterColor(BBox box)
    {
        var amount = box.data.Length;
        //int[] rgb = { Mathf.RoundToInt(box.chanels[0].count / amount), Mathf.RoundToInt(box.chanels[1].count / amount), Mathf.RoundToInt(box.chanels[2].count / amount)};
        // find the color in the box thats closest to the center
        //return FindMostSimilarColor(box.data, rgb);

        // or calculate the median color
        return new Color32((byte)(box.chanels[0].count / amount), (byte)(box.chanels[1].count / amount), (byte)(box.chanels[2].count / amount), 255);
    }

    private List<BBox> Cut(BBox box)
    {
        List<Color32> a = new List<Color32>();
        List<Color32> b = new List<Color32>();
        List<int> aIndexes = new List<int>();
        List<int> bIndexes = new List<int>();

        var index = box.max;
        var median = GetMedian(box.data, index);

        for (int i = 0, l = box.data.Length; i < l; i++)
        {
            int chanelValue = index == 0 ? box.data[i].r : (index == 1 ? box.data[i].g : box.data[i].b);

            if (chanelValue <= median)
            {
                a.Add(box.data[i]);
                aIndexes.Add(box.indexes == null ? i : box.indexes[i]);
            }
            else
            {
                b.Add(box.data[i]);
                bIndexes.Add(box.indexes == null ? i : box.indexes[i]);
            }
        }

        if (a.Count == 0 || b.Count == 0)
        {
            box.cannotCut = true;
            return null;
        }

        BBox aBox = GetBoundingBox(a.ToArray());
        BBox bBox = GetBoundingBox(b.ToArray());

        aBox.indexes = aIndexes;
        bBox.indexes = bIndexes;

        return new List<BBox> { aBox, bBox };
    }

    private int GetMedian(Color32[] data, int offset)
    {
        int[] histogram = new int[256];
        var total = 0;

        // set histogram initially to 0
        for (var i = 0; i < 256; i++)
            histogram[i] = 0;

        for (int i = 0, l = data.Length; i < l; i ++, total++)
        {
            var value = offset == 0 ? data[i].r : (offset == 1 ? data[i].g : data[i].b);
            histogram[value] += 1;
        }

        for (int i = 0, count = 0; i < histogram.Length; i++)
        {
            count += histogram[i];
            if (count > total / 2)
                return i;
        }
        return -1;
    }

    private BBox GetBoundingBox(Color32[] data)
    {

        BBox colors = new BBox { data = data };

        for (int i = 0; i < 3; i++)
        {
            Chanel chanel = new Chanel();
            colors.chanels[i] = chanel;
        }

        for (int i = 0, l = data.Length; i < l; i++)
        {
            if (data[i].r < colors.chanels[0].min) colors.chanels[0].min = data[i].r;
            if (data[i].r > colors.chanels[0].max) colors.chanels[0].max = data[i].r;
            colors.chanels[0].count += data[i].r;

            if (data[i].g < colors.chanels[1].min) colors.chanels[1].min = data[i].g;
            if (data[i].g > colors.chanels[1].max) colors.chanels[1].max = data[i].g;
            colors.chanels[1].count += data[i].g;

            if (data[i].b < colors.chanels[2].min) colors.chanels[2].min = data[i].b;
            if (data[i].b > colors.chanels[2].max) colors.chanels[2].max = data[i].b;
            colors.chanels[2].count += data[i].b;
        }

        // the count can be zero
        colors.chanels[0].distance = colors.chanels[0].count == 0 ? 0 : colors.chanels[0].max - colors.chanels[0].min;
        colors.chanels[1].distance = colors.chanels[1].count == 0 ? 0 : colors.chanels[1].max - colors.chanels[1].min;
        colors.chanels[2].distance = colors.chanels[2].count == 0 ? 0 : colors.chanels[2].max - colors.chanels[2].min;

        colors.area = Mathf.Max(colors.chanels[0].distance, 1) * Mathf.Max(colors.chanels[1].distance, 1) * Mathf.Max(colors.chanels[0].distance, 1);

        // find longest expansion
        var maxDistance = Mathf.Max(colors.chanels[0].distance, colors.chanels[1].distance, colors.chanels[2].distance);


        for (int i = 0; i < 3; i++)
        {
            if (colors.chanels[i].distance == maxDistance)
            {
                colors.max = i;
                break;
            }
        }

        return colors;
    }


    private Color32 FindMostSimilarColor(Color32[] data, int[] rgb)
    {
        var minDistance = 255 * 3;
        var index = 0;

        for (int i = 0, l = data.Length; i < l; i++)
        {
            var distance = Mathf.Abs(data[i].r - rgb[0]) + Mathf.Abs(data[i].g - rgb[1]) + Mathf.Abs(data[i].b - rgb[2]);
            if (distance < minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }

        return new Color32(data[index].r, data[index].g, data[index].b, 255);
    }
}