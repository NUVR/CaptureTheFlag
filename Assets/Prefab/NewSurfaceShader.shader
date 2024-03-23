Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // ... other state setup ...

        Pass
        {
            Stencil
            {
                Ref 2
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            // ... shader program code ...
            ENDCG
        }
    }
    // ... potentially more passes or subshaders ...
}