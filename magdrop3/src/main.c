#include "game.h"

#ifdef MAGDROP3_HAVE_RECOMP
extern const ng_func_entry_t ng_recomp_table[];
extern const size_t ng_recomp_count;
#endif

int main(int argc, char **argv) {
#ifdef MAGDROP3_HAVE_RECOMP
    return ng_main(argc, argv, &k_magdrop3,
                   ng_recomp_table, ng_recomp_count);
#else
    return ng_main(argc, argv, &k_magdrop3, NULL, 0);
#endif
}
