export class RegularExpressions {
	public static noFunkyCharactersRegex =
		"[A-Za-z0-9 \\-+*%&/\\\\()=?!:;.,üöäÄÖÜÊêéèà<>^¨\"ç'`´¦|@#°§¬¢~$£[{}\\]€]*";

	public static guidRegex =
		'^[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}$';
}
