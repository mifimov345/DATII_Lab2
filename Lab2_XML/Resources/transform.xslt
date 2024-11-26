<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html" indent="yes" />
	<xsl:template match="/">
		<html>
			<head>
				<title>Music Albums</title>
				<style>
					table { border-collapse: collapse; width: 100%; }
					th, td { border: 1px solid black; padding: 8px; }
					th { background-color: #f2f2f2; }
				</style>
			</head>
			<body>
				<h1>Music Albums</h1>
				<table>
					<tr>
						<th>Title</th>
						<th>Genres</th>
						<th>Artists</th>
						<th>Release Date</th>
						<th>Age Restriction</th>
					</tr>
					<xsl:for-each select="albums/album">
						<tr>
							<td>
								<xsl:value-of select="title" />
							</td>
							<td>
								<xsl:for-each select="genres/genre">
									<xsl:value-of select="." />
									<xsl:if test="position() != last()">, </xsl:if>
								</xsl:for-each>
							</td>
							<td>
								<xsl:for-each select="artists/artist">
									<xsl:value-of select="." />
									<xsl:if test="position() != last()">, </xsl:if>
								</xsl:for-each>
							</td>
							<td>
								<xsl:value-of select="releaseDate" />
							</td>
							<td>
								<xsl:value-of select="ageRestriction" />
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
