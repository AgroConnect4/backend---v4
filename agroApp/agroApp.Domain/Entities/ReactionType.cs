using System;
using System.ComponentModel.DataAnnotations.Schema;
using agroApp.Domain.Entities;

namespace agroApp.Domain.Entities
{
    public enum ReactionType
    {
        Like,
        Love,
        Haha,
        Wow,
        Sad,
        Angry
    }
}