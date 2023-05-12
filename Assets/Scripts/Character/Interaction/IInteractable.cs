using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionMethod : byte { ByKey, ByMenu, ByBreak, ByBump };

public interface IInteractable
{
    InteractionMethod Method { get; }

    void Interact();
}
