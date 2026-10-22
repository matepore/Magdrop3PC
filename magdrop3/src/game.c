#include "game.h"

const ng_game_t k_magdrop3 = {
    .name = "magdrop3",
    .title = "Magical Drop III",

    .prom_size = 0x100000,
    .prom = {
        {"233-p1.bin", 0x000000, 0x000000, 0x100000, 1},
    },

    .crom_size = 0x1000000,
    .crom_pairs = {
        {"233-c1.bin", "233-c2.bin"},
        {"233-c3.bin", "233-c4.bin"},
    },

    .srom = "233-s1.bin",
    .m1 = "233-m1.bin",

    .vrom = {
        "233-v1.bin",
        "233-v2.bin",
    },
};