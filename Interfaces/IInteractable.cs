using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    string InteractionPrompt { get; }
    float InteractionRange { get; }
    void Interact(MovimentarJogador player);
    bool CanInteract(MovimentarJogador player);
}
