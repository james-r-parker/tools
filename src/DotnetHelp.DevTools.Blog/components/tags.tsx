import Link from "next/link";

export default function Tags({ tags }: { tags: string[] }) {
    return (
        <>
            {tags.map((tag) => <Link key={tag} href={`/blog/tag/${encodeURIComponent(tag)}`}><span className="text-xs bg-sky-100 dark:bg-sky-800 text-sky-500 dark:text-sky-400 rounded-full px-2 py-1 mr-2">{tag}</span></Link>)}
        </>
    );
}