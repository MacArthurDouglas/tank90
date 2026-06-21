namespace Tank90
{
    /// <summary>
    /// Stage maps. 13x13 cells, one char per cell, line 0 = top row.
    /// '.' empty  'B' brick  'S' steel  'W' water  'T' trees  'I' ice  'E' eagle/base.
    /// Enemy spawns: top cols 0,6,12. Player spawn: bottom cols 4,8.
    /// </summary>
    public static class LevelData
    {
        public static readonly string[][] Stages =
        {
            // Stage 1
            new[]
            {
                ".............",
                ".BB.BB.BB.BB.",
                ".BB.BB.BB.BB.",
                ".............",
                "BB..BB.BB..BB",
                "BB..SS.SS..BB",
                "....S...S....",
                "....S...S....",
                ".WW.......WW.",
                ".WW.BBBBB.WW.",
                ".............",
                ".....BBB.....",
                ".....BEB.....",
            },
            // Stage 2
            new[]
            {
                "..S.......S..",
                "..S.BBBBB.S..",
                "..S.B...B.S..",
                "....B.T.B....",
                "BBB.B.T.B.BBB",
                "..W.....W....",
                "..W.WWWWW.W..",
                "..W.....W....",
                "BBB.B.T.B.BBB",
                "....B.T.B....",
                "..S.B...B.S..",
                "..S.BBBBB.S..",
                ".....BEB.....",
            },
            // Stage 3
            new[]
            {
                "B.B.B.B.B.B.B",
                ".T.T.T.T.T.T.",
                "BBBB.....BBBB",
                "...S.WWW.S...",
                ".B.S.W.W.S.B.",
                ".B.S.W.W.S.B.",
                ".B.......S.B.",
                ".B.SSSSS.S.B.",
                ".B.......S.B.",
                "II.........II",
                "II..BBBBB..II",
                ".....B.B.....",
                ".....BEB.....",
            },
        };
    }
}
