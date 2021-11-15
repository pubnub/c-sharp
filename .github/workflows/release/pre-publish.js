const process = require('process');
const path = require('path');
const fs = require('fs');



/**
 * Capitalize first character of passed string.
 *
 * @param {string} str Source string for which first letter should be capitalized.
 *
 * @return {string} Capitalized string.
 */
function capitalizedString(str) {
  return `${str.replace(/^./, str[0].toUpperCase())}`;
}

/**
 * Replace specified
 * @param {string} src Source string in which replacement should be done.
 * @param {string} str String which should be replaced.
 * @param {string} replacement String which should be used to replace `str`.
 * @param {number|undefined} index Index in string starting from which replacement should happen.
 *
 * @return {string} Modified string with replaced pieces.
 */
function replaceString(src, str, replacement, index) {
  if (index === undefined) return src.replace(new RegExp(str, 'g'), replacement)
  else return src.substring(0, index) + replacement + src.substring(index + str.length)
}



// Build change log entries suitable for project files update.
const changelogPath = path.join(process.env['GITHUB_WORKSPACE'], '.github/.release', 'changelog.json');
const changelogContent = fs.readFileSync(changelogPath).toString('utf8');
let changeEntries = [];
let changelog;

if (changelogContent) {
	try {
    changelog = JSON.parse(changelogContent);

	} catch (error) {
    console.error(`Unable to parse changelog file content: '${error}'`);
    process.exit(1);
	}
} else {
  console.error(`Changelog information is missing: '${changelogPath}'`);
  process.exit(2);
}


if (changelog && changelog.entries.length === 0) {
  console.error('Changelog doesn\'t contain any changes.');
  process.exit(3);
}


// Iterate over list of entries to compose final list of changes.
for (let change of changelog.entries) {
  let description = change.description || change.title;
  if (description.endsWith('.'))
    description = description.slice(0, description.length - 1);
  if (change.isBreaking) description = `BREAKING CHANGES: ${description}`;

  changeEntries.push(`${capitalizedString(description)}.`);
}



// Update C# project files 'PackageReleaseNotes' nodes.
const nodeMatcher = new RegExp('<PackageReleaseNotes>((.|[\r\n])+)</PackageReleaseNotes>', 'gm');
const nodeChangelog = changeEntries.join('\n');
const localCopyPath = process.argv.pop()
const projectPaths = [
  'src/Api/PubnubApi/PubnubApi.csproj',
  'src/Api/PubnubApiPCL/PubnubApiPCL.csproj',
  'src/Api/PubnubApiUWP/PubnubApiUWP.csproj'
];

for (var projectIdx = projectPaths.length - 1; projectIdx >= 0; projectIdx--) {
	const projectFilePath = path.join(localCopyPath, projectPaths[projectIdx]);
  let match;

	if (fs.existsSync(projectFilePath)) {
    let projectContent = fs.readFileSync(projectFilePath).toString('utf8');

    if (projectContent) {
      while ((match = nodeMatcher.exec(projectContent)) !== null) {
      	const updated = match[0].replace(match[1], nodeChangelog)
        projectContent = replaceString(projectContent, match[0], updated, match.index)
        fs.writeFileSync(projectFilePath, projectContent, {encoding: 'utf8'})
      }
    }
	}
}
