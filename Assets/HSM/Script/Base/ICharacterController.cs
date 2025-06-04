using System.Collections.Generic;
using UnityEngine;

public interface ICharacterController
{
    void SetParameters(Dictionary<string, float> data);
    void ApplyParameters();
}
