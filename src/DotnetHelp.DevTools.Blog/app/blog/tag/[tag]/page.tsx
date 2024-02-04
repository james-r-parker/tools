import { PostOverview } from "@/components/overview";
import Title from "@/components/title";
import { listBlogPostsByTag } from "@/lib/blog";

export default async function Tag({ params }: { params: { tag: string } }) {
    const tag = decodeURIComponent(params.tag);
    const pages = await listBlogPostsByTag(tag);

    return <article>
        <Title title="Blog" overview={`${tag} Blog Posts`} />
        <div className="mt-5 grid gap-5 md:grid-cols-2">
            {pages.map((post, index) => <PostOverview key={index} post={post} />)}
        </div>
    </article>
}