# Hunger Revamped mod for *The Long Dark*

*The Long Dark's* hunger system has some problems:
- It can't differentiate between starving for a few days and starving for months
- It allows players to get by on 600 calories per day, forever
- Calorie deficits don't need to be compensated
- Long-time starvation barely affects the player

### Hunger Revamped

Despite what Hinterland claims, there is simply no way to fix these problems
with just one hunger variable - there is just not enough information.
That's why Hunger Revamped **splits nourishment into 'hunger' and 'stored calories'**.

> ![Hunger and stored calories](Images/hunger_and_stored_calories.png)

**Hunger** drains over time and is filled up by eating, just like in the regular game.
When the hunger bar sinks below 20%, you'll slowly start to take starvation damage.
How much damage you take depends on how hungry you are: At first, you'll barely take any damage.
But if you starve yourself for too long, you can take up to 5% damage per hour.

**Stored calories**, on the other hand, represent the player's body fat and thus change rather slowly.
In total, one can store up to 20 000 calories in body fat, which is about a week's worth of energy.
Staying well-nourished by keeping the hunger bar full (indicated by a small, green plus sign) for some time will slowly accumulate stored calories.
If you're hungry (indicated by a red minus sign), on the other hand, the stored calories are slowly drained into the hunger bar.

That means that if you have lots and lots of stored calories, your hunger bar won't empty all the way,
shielding you from most starvation damage. If you have very few stored calories,
on the other hand, you'll find yourself starving very quickly.

> ![Hunger bar fill levels](Images/hunger_bar_fill_levels.png)

Finally, having lots of stored calories (read: body fat) also gives you a small warmth bonus of up to 2 °C.
But if you've used up your calorie store, your body won't have enough energy to keep your body warm,
resulting in a reduction of your "Feels like" temperature by up to 3 °C.

### Gameplay Effects

#### No more hibernation

Calories can no longer simply be "lost" by completely draining the hunger bar.
The emptier the hunger bar gets, the more calories will be consumed from the calorie store instead.

You *can* still starve yourself during the day and eat before sleeping to regain
some health – but only for some time. The longer you starve yourself, the emptier
your calorie store gets and the faster your hunger bar will drain. This leads to
more and more starvation damage, which you won't be able to compensate for by sleeping.

#### Keeping yourself fed is a challenge

Only having to consume 600 calories per day really made gathering enough food to
survive trivial, even in Interloper. By not being able to cheat the hunger system,
survival is much more of a challenge.

If HungerRevamped still doesn't make this aspect of the game interesting enough,
consider getting [WildlifeBegone](https://github.com/zeobviouslyfakeacc/WildlifeBegone/releases).
This mod makes wildlife much rarer, delaying (or maybe even getting rid of) that point
in the game where you find yourself with a huge pile of food and nothing to do.

#### Start of the game

You'll start with 12 000 stored calories. That means that you'll have a buffer period at the start
of the game during which you don't need to consume much food. This is especially useful in Interloper,
where you find very little food until you can craft a bow and hunt game.

If you're playing a custom mode game, you can also customize how many calories you start with.

#### Travelling

Having many stored calories also comes in handy when you want to travel to – or find loot in – another region.
Thanks to those stored calories, you won't need to bring as much food, so you'll have more space to carry
more important items. The warmth bonus certainly also helps when you need to spend lots of time outside.

#### Sleeping, fishing, harvesting, breaking down objects

When interacting with the world, you were usually shown how many calories
you'd burn and how many calories were still left in your hunger bar.
With HungerRevamped installed, these two stats aren't of much use.
Instead, all of these screens will now show how full your hunger bar is
and how many calories are stored in body fat **after** that interaction.

This is especially important when **sleeping**. To let your health regenerate,
you need to avoid starvation. You should thus ensure that your hunger bar stays
more than 20% full – if possible.

#### Cooking Skill Points

Cooking ruined food gives no skill points. Cooking a partial portion (less than 1 kilogram) of meat gives a corresponding
chance of awarding a skill. In other words, cooking 5 kilograms of meat gives 5 skill points (on average) regardless
of how you cook it.

#### Delayed Food Poisoning

Hunger Revamped implements a delayed food poisoning system. This means that you won't get food poisoning right away,
but instead it shows several hours later. 

#### Realistic Food Poisoning Setting

In addition, there is a realistic food poisoning setting which makes the probability of food poisoning gradually 
increase as condition decreases. Under this setting, food poisoning probability is also scaled by the proportion eaten. 
In other words, eating half as much is half as likely to give you food poisoning. This setting also disables the 
strange immunity to food poisoning at Cooking Level 5;

#### Ruined Food Settings

There are settings to make ruined food inedible and/or uncookable.

#### Calorie Burn Rate Setting

In cold weather environments such as Antarctica, researchers often have to consume significantly more calories than they
otherwise would in warm environments. In respect to that reality, there is a setting to scale your daily calorie needs.

#### Harvestable Cans

Canned food items can be harvested for their recycled can if the player cannot (or doesn't want to) eat them.

### Installation

1. If you haven't done so already, install MelonLoader by downloading and running [MelonLoader.Installer.exe](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe)
2. If you haven't done so already, install [ModSettings](https://github.com/zeobviouslyfakeacc/ModSettings) v1.6 or newer
3. Download the latest version of `HungerRevamped.dll` from the [releases page](https://github.com/zeobviouslyfakeacc/HungerRevamped/releases)
4. Move `HungerRevamped.dll` into the Mods folder in your TLD install directory

You can install and use Hunger Revamped in old saves. You'll start with 10 000 stored calories.

You can always uninstall Hunger Revamped and continue playing your save like usual, too,
but the amount of calories stored in body fat will be lost.

### Recommendations

- Don't play on Pilgrim. A better hunger system really doesn't matter if you only consume 50 calories per hour.
- Consider getting [EnableStatusBarPercentages](https://github.com/zeobviouslyfakeacc/EnableStatusBarPercentages/releases).
  It lets you see the exact fill levels of your status bar in the status / first aid screen.
