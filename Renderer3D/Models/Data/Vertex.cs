﻿using System.Numerics;

namespace Renderer3D.Models.Data
{
    public struct Vertex
    {
        /// <summary>
        /// Original coordinates translated to screen space using translation matrices
        /// </summary>
        public Vector3 Coordinates;

        /// <summary>
        /// Normal vector of the vertex
        /// </summary>
        public Vector3 Normal;
    }
}
