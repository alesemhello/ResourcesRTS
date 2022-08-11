using System.Collections.Generic;

public class Character : Unit
{
    public Character(CharacterData data) :
        base(data, new List<ResourceValue>() { })
    { }
}