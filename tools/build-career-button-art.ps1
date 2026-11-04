param()

# NOTE: this script no longer generates any title artwork.
#
# InterfaceResources/XuanhuangSkin/title_beast_talents.png ("本命神通") is now
# produced directly in Codex with its built-in drawing, matching the existing
# 修士档案 (title_biography) and 职业技能 (title_career_skills) 3D gold-embossed
# art family. This script must NOT render or overwrite that file.
#
# The career titles (title_career_skills / title_career_archive) are painted by
# tools/integrate-living-art.py as large ~1450x441 art and are not produced here
# either.
#
# The old flat 320x80 teal-outline font render has been removed because it did
# not belong to the career-title visual family and could clobber the Codex art.

Write-Host "title_beast_talents.png is produced by Codex built-in drawing; this script does not generate or overwrite any title art."
